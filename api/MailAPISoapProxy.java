package vn.vtcebank.mail.api;

public class MailAPISoapProxy implements vn.vtcebank.mail.api.MailAPISoap {
  private String _endpoint = null;
  private vn.vtcebank.mail.api.MailAPISoap mailAPISoap = null;
  
  public MailAPISoapProxy() {
    _initMailAPISoapProxy();
  }
  
  public MailAPISoapProxy(String endpoint) {
    _endpoint = endpoint;
    _initMailAPISoapProxy();
  }
  
  private void _initMailAPISoapProxy() {
    try {
      mailAPISoap = (new vn.vtcebank.mail.api.MailAPILocator()).getMailAPISoap();
      if (mailAPISoap != null) {
        if (_endpoint != null)
          ((javax.xml.rpc.Stub)mailAPISoap)._setProperty("javax.xml.rpc.service.endpoint.address", _endpoint);
        else
          _endpoint = (String)((javax.xml.rpc.Stub)mailAPISoap)._getProperty("javax.xml.rpc.service.endpoint.address");
      }
      
    }
    catch (javax.xml.rpc.ServiceException serviceException) {}
  }
  
  public String getEndpoint() {
    return _endpoint;
  }
  
  public void setEndpoint(String endpoint) {
    _endpoint = endpoint;
    if (mailAPISoap != null)
      ((javax.xml.rpc.Stub)mailAPISoap)._setProperty("javax.xml.rpc.service.endpoint.address", _endpoint);
    
  }
  
  public vn.vtcebank.mail.api.MailAPISoap getMailAPISoap() {
    if (mailAPISoap == null)
      _initMailAPISoapProxy();
    return mailAPISoap;
  }
  
  public int checkEmailExist(java.lang.String email, int idAuthen, java.lang.String codeAuthen) throws java.rmi.RemoteException{
    if (mailAPISoap == null)
      _initMailAPISoapProxy();
    return mailAPISoap.checkEmailExist(email, idAuthen, codeAuthen);
  }
  
  public int sendMail(java.lang.String fromName, java.lang.String fromEmail, java.lang.String toEmail, java.lang.String subject, java.lang.String message, int eventMail, long idRequest, int dateSend, int idAuthen, java.lang.String codeAuthen) throws java.rmi.RemoteException{
    if (mailAPISoap == null)
      _initMailAPISoapProxy();
    return mailAPISoap.sendMail(fromName, fromEmail, toEmail, subject, message, eventMail, idRequest, dateSend, idAuthen, codeAuthen);
  }
  
  public int sendMailNew(java.lang.String fromName, java.lang.String fromEmail, java.lang.String toEmail, java.lang.String subject, java.lang.String message, int eventMail, long idRequest, int dateSend, int idAuthen, java.lang.String codeAuthen, java.lang.String accountName, long accountID) throws java.rmi.RemoteException{
    if (mailAPISoap == null)
      _initMailAPISoapProxy();
    return mailAPISoap.sendMailNew(fromName, fromEmail, toEmail, subject, message, eventMail, idRequest, dateSend, idAuthen, codeAuthen, accountName, accountID);
  }
  
  public int sendMaileBankPaygate(java.lang.String toEmail, java.lang.String accountName, long accountID, int contentID, java.lang.String param) throws java.rmi.RemoteException{
    if (mailAPISoap == null)
      _initMailAPISoapProxy();
    return mailAPISoap.sendMaileBankPaygate(toEmail, accountName, accountID, contentID, param);
  }
  
  public int sendMailMonitor(java.lang.String fromName, java.lang.String fromEmail, java.lang.String toEmail, java.lang.String ccEmail, java.lang.String subject, java.lang.String message, int eventMail, long idRequest, int idAuthen, java.lang.String codeAuthen) throws java.rmi.RemoteException{
    if (mailAPISoap == null)
      _initMailAPISoapProxy();
    return mailAPISoap.sendMailMonitor(fromName, fromEmail, toEmail, ccEmail, subject, message, eventMail, idRequest, idAuthen, codeAuthen);
  }
  
  
}