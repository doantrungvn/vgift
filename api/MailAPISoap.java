/**
 * MailAPISoap.java
 *
 * This file was auto-generated from WSDL
 * by the Apache Axis 1.4 Apr 22, 2006 (06:55:48 PDT) WSDL2Java emitter.
 */

package vn.vtcebank.mail.api;

public interface MailAPISoap extends java.rmi.Remote {
    public int checkEmailExist(java.lang.String email, int idAuthen, java.lang.String codeAuthen) throws java.rmi.RemoteException;
    public int sendMail(java.lang.String fromName, java.lang.String fromEmail, java.lang.String toEmail, java.lang.String subject, java.lang.String message, int eventMail, long idRequest, int dateSend, int idAuthen, java.lang.String codeAuthen) throws java.rmi.RemoteException;
    public int sendMailNew(java.lang.String fromName, java.lang.String fromEmail, java.lang.String toEmail, java.lang.String subject, java.lang.String message, int eventMail, long idRequest, int dateSend, int idAuthen, java.lang.String codeAuthen, java.lang.String accountName, long accountID) throws java.rmi.RemoteException;
    public int sendMaileBankPaygate(java.lang.String toEmail, java.lang.String accountName, long accountID, int contentID, java.lang.String param) throws java.rmi.RemoteException;
    public int sendMailMonitor(java.lang.String fromName, java.lang.String fromEmail, java.lang.String toEmail, java.lang.String ccEmail, java.lang.String subject, java.lang.String message, int eventMail, long idRequest, int idAuthen, java.lang.String codeAuthen) throws java.rmi.RemoteException;
}
