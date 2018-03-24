$connectionName = "AzureRunAsConnection"

try
{
    # Get the connection "AzureRunAsConnection "
    $servicePrincipalConnection = Get-AutomationConnection -Name $connectionName         

    "Logging in to Azure..."
    Add-AzureRmAccount `
        -ServicePrincipal `
        -TenantId $servicePrincipalConnection.TenantId `
        -ApplicationId $servicePrincipalConnection.ApplicationId `
        -CertificateThumbprint $servicePrincipalConnection.CertificateThumbprint

    "Login complete."
}
catch {
    if (!$servicePrincipalConnection)
    {
        $ErrorMessage = "Connection $connectionName not found."
        throw $ErrorMessage
    } else{
        Write-Error -Message $_.Exception
        throw $_.Exception
    }
}

function Roll-ServiceBusKey($resourceGroupName, $vaultName, $serviceBusNamespaceName, $serviceBusAccessPolicyName, $secretName)
{
    Write-Output "Rolling authentication keys for Service Bus namespace '$serviceBusNamespaceName' with access policy '$serviceBusAccessPolicyName'"

    # Roll secondary key
    New-AzureRmServiceBusKey -ResourceGroup $resourceGroupName -Namespace $serviceBusNamespaceName -Name $serviceBusAccessPolicyName -RegenerateKey SecondaryKey
    Write-Output "Secondary key rolled"
    $policyKeys = Get-AzureRmServiceBusKey -Namespace $serviceBusNamespaceName -Name $serviceBusAccessPolicyName -ResourceGroup $resourceGroupName
    
    # Update secret in Key Vault with new secondary key
    $secretValue = ConvertTo-SecureString $policyKeys.SecondaryConnectionString -AsPlainText -Force
    $secret = Set-AzureKeyVaultSecret -vaultName $vaultName -Name $secretName -secretValue $secretValue
    Write-Output "Changed secret '$secretName' to use secondary key for now (New version '$($secret.Version)')"
    
    # Roll primary key
    New-AzureRmServiceBusKey -Namespace $serviceBusNamespaceName -Name $serviceBusAccessPolicyName -RegenerateKey PrimaryKey -ResourceGroup $resourceGroupName
    Write-Output "Primary key rolled"
    $policyKeys = Get-AzureRmServiceBusKey -Namespace $serviceBusNamespaceName -Name $serviceBusAccessPolicyName -ResourceGroup $resourceGroupName
    
    # Update secret in Key Vault with new primary key
    $secretValue = ConvertTo-SecureString $policyKeys.PrimaryConnectionString -AsPlainText -Force
    $secret = Set-AzureKeyVaultSecret -vaultName $vaultName -Name $secretName -secretValue $secretValue
    Write-Output "Changed secret '$secretName' to use primary key again (New version '$($secret.Version)')"

    Write-Output "Authentication keys rolled for Service Bus namespace '$serviceBusNamespaceName' with access policy '$serviceBusAccessPolicyName'"
}

# Example of rolling keys
Roll-ServiceBusKey -resourceGroupName 'secure-applications-with-key-vault' -vaultName 'secure-applications' -serviceBusNamespaceName 'secure-applications-with-key-vault' -serviceBusAccessPolicyName 'API' -secretName 'ServiceBus-ConnectionString'