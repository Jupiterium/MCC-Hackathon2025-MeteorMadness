using UnityEngine.Networking;

public class AcceptAllCertificatesSignedWithASpecificKeyPublicKey : CertificateHandler
{
    // This will simply accept all certificates (for development use only)
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}
