using UnityEngine.Networking;

public class WebReqSkipCert : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}
