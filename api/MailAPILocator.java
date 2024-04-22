/**
 * MailAPILocator.java
 *
 * This file was auto-generated from WSDL
 * by the Apache Axis 1.4 Apr 22, 2006 (06:55:48 PDT) WSDL2Java emitter.
 */

package vn.vtcebank.mail.api;

public class MailAPILocator extends org.apache.axis.client.Service implements vn.vtcebank.mail.api.MailAPI {

    public MailAPILocator() {
    }


    public MailAPILocator(org.apache.axis.EngineConfiguration config) {
        super(config);
    }

    public MailAPILocator(java.lang.String wsdlLoc, javax.xml.namespace.QName sName) throws javax.xml.rpc.ServiceException {
        super(wsdlLoc, sName);
    }

    // Use to get a proxy class for MailAPISoap
    private java.lang.String MailAPISoap_address = "http://mailservice.vtcebank.vn/mailservice/MailAPI.asmx";

    public java.lang.String getMailAPISoapAddress() {
        return MailAPISoap_address;
    }

    // The WSDD service name defaults to the port name.
    private java.lang.String MailAPISoapWSDDServiceName = "MailAPISoap";

    public java.lang.String getMailAPISoapWSDDServiceName() {
        return MailAPISoapWSDDServiceName;
    }

    public void setMailAPISoapWSDDServiceName(java.lang.String name) {
        MailAPISoapWSDDServiceName = name;
    }

    public vn.vtcebank.mail.api.MailAPISoap getMailAPISoap() throws javax.xml.rpc.ServiceException {
       java.net.URL endpoint;
        try {
            endpoint = new java.net.URL(MailAPISoap_address);
        }
        catch (java.net.MalformedURLException e) {
            throw new javax.xml.rpc.ServiceException(e);
        }
        return getMailAPISoap(endpoint);
    }

    public vn.vtcebank.mail.api.MailAPISoap getMailAPISoap(java.net.URL portAddress) throws javax.xml.rpc.ServiceException {
        try {
            vn.vtcebank.mail.api.MailAPISoapStub _stub = new vn.vtcebank.mail.api.MailAPISoapStub(portAddress, this);
            _stub.setPortName(getMailAPISoapWSDDServiceName());
            return _stub;
        }
        catch (org.apache.axis.AxisFault e) {
            return null;
        }
    }

    public void setMailAPISoapEndpointAddress(java.lang.String address) {
        MailAPISoap_address = address;
    }

    /**
     * For the given interface, get the stub implementation.
     * If this service has no port for the given interface,
     * then ServiceException is thrown.
     */
    public java.rmi.Remote getPort(Class serviceEndpointInterface) throws javax.xml.rpc.ServiceException {
        try {
            if (vn.vtcebank.mail.api.MailAPISoap.class.isAssignableFrom(serviceEndpointInterface)) {
                vn.vtcebank.mail.api.MailAPISoapStub _stub = new vn.vtcebank.mail.api.MailAPISoapStub(new java.net.URL(MailAPISoap_address), this);
                _stub.setPortName(getMailAPISoapWSDDServiceName());
                return _stub;
            }
        }
        catch (java.lang.Throwable t) {
            throw new javax.xml.rpc.ServiceException(t);
        }
        throw new javax.xml.rpc.ServiceException("There is no stub implementation for the interface:  " + (serviceEndpointInterface == null ? "null" : serviceEndpointInterface.getName()));
    }

    /**
     * For the given interface, get the stub implementation.
     * If this service has no port for the given interface,
     * then ServiceException is thrown.
     */
    public java.rmi.Remote getPort(javax.xml.namespace.QName portName, Class serviceEndpointInterface) throws javax.xml.rpc.ServiceException {
        if (portName == null) {
            return getPort(serviceEndpointInterface);
        }
        java.lang.String inputPortName = portName.getLocalPart();
        if ("MailAPISoap".equals(inputPortName)) {
            return getMailAPISoap();
        }
        else  {
            java.rmi.Remote _stub = getPort(serviceEndpointInterface);
            ((org.apache.axis.client.Stub) _stub).setPortName(portName);
            return _stub;
        }
    }

    public javax.xml.namespace.QName getServiceName() {
        return new javax.xml.namespace.QName("http://api.mail.vtcebank.vn/", "MailAPI");
    }

    private java.util.HashSet ports = null;

    public java.util.Iterator getPorts() {
        if (ports == null) {
            ports = new java.util.HashSet();
            ports.add(new javax.xml.namespace.QName("http://api.mail.vtcebank.vn/", "MailAPISoap"));
        }
        return ports.iterator();
    }

    /**
    * Set the endpoint address for the specified port name.
    */
    public void setEndpointAddress(java.lang.String portName, java.lang.String address) throws javax.xml.rpc.ServiceException {
        
if ("MailAPISoap".equals(portName)) {
            setMailAPISoapEndpointAddress(address);
        }
        else 
{ // Unknown Port Name
            throw new javax.xml.rpc.ServiceException(" Cannot set Endpoint Address for Unknown Port" + portName);
        }
    }

    /**
    * Set the endpoint address for the specified port name.
    */
    public void setEndpointAddress(javax.xml.namespace.QName portName, java.lang.String address) throws javax.xml.rpc.ServiceException {
        setEndpointAddress(portName.getLocalPart(), address);
    }

}
