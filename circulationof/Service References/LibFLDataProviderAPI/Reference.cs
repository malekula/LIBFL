﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Circulation.LibFLDataProviderAPI {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ReaderInfo", Namespace="http://tempuri.org/")]
    [System.SerializableAttribute()]
    public partial class ReaderInfo : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FamilyNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FatherNameField;
        
        private System.DateTime DateBirthField;
        
        private bool IsRemoteReaderField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public string FamilyName {
            get {
                return this.FamilyNameField;
            }
            set {
                if ((object.ReferenceEquals(this.FamilyNameField, value) != true)) {
                    this.FamilyNameField = value;
                    this.RaisePropertyChanged("FamilyName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public string Name {
            get {
                return this.NameField;
            }
            set {
                if ((object.ReferenceEquals(this.NameField, value) != true)) {
                    this.NameField = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=2)]
        public string FatherName {
            get {
                return this.FatherNameField;
            }
            set {
                if ((object.ReferenceEquals(this.FatherNameField, value) != true)) {
                    this.FatherNameField = value;
                    this.RaisePropertyChanged("FatherName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=3)]
        public System.DateTime DateBirth {
            get {
                return this.DateBirthField;
            }
            set {
                if ((this.DateBirthField.Equals(value) != true)) {
                    this.DateBirthField = value;
                    this.RaisePropertyChanged("DateBirth");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=4)]
        public bool IsRemoteReader {
            get {
                return this.IsRemoteReaderField;
            }
            set {
                if ((this.IsRemoteReaderField.Equals(value) != true)) {
                    this.IsRemoteReaderField = value;
                    this.RaisePropertyChanged("IsRemoteReader");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="LibFLDataProviderAPI.ServiceSoap")]
    public interface ServiceSoap {
        
        // CODEGEN: Generating message contract since element name NumberReader from namespace http://tempuri.org/ is not marked nillable
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetReaderInfo", ReplyAction="*")]
        Circulation.LibFLDataProviderAPI.GetReaderInfoResponse GetReaderInfo(Circulation.LibFLDataProviderAPI.GetReaderInfoRequest request);
        
        // CODEGEN: Generating message contract since element name login from namespace http://tempuri.org/ is not marked nillable
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/Authorize", ReplyAction="*")]
        Circulation.LibFLDataProviderAPI.AuthorizeResponse Authorize(Circulation.LibFLDataProviderAPI.AuthorizeRequest request);
        
        // CODEGEN: Generating message contract since element name login from namespace http://tempuri.org/ is not marked nillable
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetLoginType", ReplyAction="*")]
        Circulation.LibFLDataProviderAPI.GetLoginTypeResponse GetLoginType(Circulation.LibFLDataProviderAPI.GetLoginTypeRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class GetReaderInfoRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="GetReaderInfo", Namespace="http://tempuri.org/", Order=0)]
        public Circulation.LibFLDataProviderAPI.GetReaderInfoRequestBody Body;
        
        public GetReaderInfoRequest() {
        }
        
        public GetReaderInfoRequest(Circulation.LibFLDataProviderAPI.GetReaderInfoRequestBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://tempuri.org/")]
    public partial class GetReaderInfoRequestBody {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string NumberReader;
        
        public GetReaderInfoRequestBody() {
        }
        
        public GetReaderInfoRequestBody(string NumberReader) {
            this.NumberReader = NumberReader;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class GetReaderInfoResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="GetReaderInfoResponse", Namespace="http://tempuri.org/", Order=0)]
        public Circulation.LibFLDataProviderAPI.GetReaderInfoResponseBody Body;
        
        public GetReaderInfoResponse() {
        }
        
        public GetReaderInfoResponse(Circulation.LibFLDataProviderAPI.GetReaderInfoResponseBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://tempuri.org/")]
    public partial class GetReaderInfoResponseBody {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public Circulation.LibFLDataProviderAPI.ReaderInfo GetReaderInfoResult;
        
        public GetReaderInfoResponseBody() {
        }
        
        public GetReaderInfoResponseBody(Circulation.LibFLDataProviderAPI.ReaderInfo GetReaderInfoResult) {
            this.GetReaderInfoResult = GetReaderInfoResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class AuthorizeRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="Authorize", Namespace="http://tempuri.org/", Order=0)]
        public Circulation.LibFLDataProviderAPI.AuthorizeRequestBody Body;
        
        public AuthorizeRequest() {
        }
        
        public AuthorizeRequest(Circulation.LibFLDataProviderAPI.AuthorizeRequestBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://tempuri.org/")]
    public partial class AuthorizeRequestBody {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string login;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=1)]
        public string password;
        
        public AuthorizeRequestBody() {
        }
        
        public AuthorizeRequestBody(string login, string password) {
            this.login = login;
            this.password = password;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class AuthorizeResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="AuthorizeResponse", Namespace="http://tempuri.org/", Order=0)]
        public Circulation.LibFLDataProviderAPI.AuthorizeResponseBody Body;
        
        public AuthorizeResponse() {
        }
        
        public AuthorizeResponse(Circulation.LibFLDataProviderAPI.AuthorizeResponseBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://tempuri.org/")]
    public partial class AuthorizeResponseBody {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string AuthorizeResult;
        
        public AuthorizeResponseBody() {
        }
        
        public AuthorizeResponseBody(string AuthorizeResult) {
            this.AuthorizeResult = AuthorizeResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class GetLoginTypeRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="GetLoginType", Namespace="http://tempuri.org/", Order=0)]
        public Circulation.LibFLDataProviderAPI.GetLoginTypeRequestBody Body;
        
        public GetLoginTypeRequest() {
        }
        
        public GetLoginTypeRequest(Circulation.LibFLDataProviderAPI.GetLoginTypeRequestBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://tempuri.org/")]
    public partial class GetLoginTypeRequestBody {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string login;
        
        public GetLoginTypeRequestBody() {
        }
        
        public GetLoginTypeRequestBody(string login) {
            this.login = login;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class GetLoginTypeResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="GetLoginTypeResponse", Namespace="http://tempuri.org/", Order=0)]
        public Circulation.LibFLDataProviderAPI.GetLoginTypeResponseBody Body;
        
        public GetLoginTypeResponse() {
        }
        
        public GetLoginTypeResponse(Circulation.LibFLDataProviderAPI.GetLoginTypeResponseBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://tempuri.org/")]
    public partial class GetLoginTypeResponseBody {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string GetLoginTypeResult;
        
        public GetLoginTypeResponseBody() {
        }
        
        public GetLoginTypeResponseBody(string GetLoginTypeResult) {
            this.GetLoginTypeResult = GetLoginTypeResult;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ServiceSoapChannel : Circulation.LibFLDataProviderAPI.ServiceSoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ServiceSoapClient : System.ServiceModel.ClientBase<Circulation.LibFLDataProviderAPI.ServiceSoap>, Circulation.LibFLDataProviderAPI.ServiceSoap {
        
        public ServiceSoapClient() {
        }
        
        public ServiceSoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ServiceSoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ServiceSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ServiceSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        Circulation.LibFLDataProviderAPI.GetReaderInfoResponse Circulation.LibFLDataProviderAPI.ServiceSoap.GetReaderInfo(Circulation.LibFLDataProviderAPI.GetReaderInfoRequest request) {
            return base.Channel.GetReaderInfo(request);
        }
        
        public Circulation.LibFLDataProviderAPI.ReaderInfo GetReaderInfo(string NumberReader) {
            Circulation.LibFLDataProviderAPI.GetReaderInfoRequest inValue = new Circulation.LibFLDataProviderAPI.GetReaderInfoRequest();
            inValue.Body = new Circulation.LibFLDataProviderAPI.GetReaderInfoRequestBody();
            inValue.Body.NumberReader = NumberReader;
            Circulation.LibFLDataProviderAPI.GetReaderInfoResponse retVal = ((Circulation.LibFLDataProviderAPI.ServiceSoap)(this)).GetReaderInfo(inValue);
            return retVal.Body.GetReaderInfoResult;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        Circulation.LibFLDataProviderAPI.AuthorizeResponse Circulation.LibFLDataProviderAPI.ServiceSoap.Authorize(Circulation.LibFLDataProviderAPI.AuthorizeRequest request) {
            return base.Channel.Authorize(request);
        }
        
        public string Authorize(string login, string password) {
            Circulation.LibFLDataProviderAPI.AuthorizeRequest inValue = new Circulation.LibFLDataProviderAPI.AuthorizeRequest();
            inValue.Body = new Circulation.LibFLDataProviderAPI.AuthorizeRequestBody();
            inValue.Body.login = login;
            inValue.Body.password = password;
            Circulation.LibFLDataProviderAPI.AuthorizeResponse retVal = ((Circulation.LibFLDataProviderAPI.ServiceSoap)(this)).Authorize(inValue);
            return retVal.Body.AuthorizeResult;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        Circulation.LibFLDataProviderAPI.GetLoginTypeResponse Circulation.LibFLDataProviderAPI.ServiceSoap.GetLoginType(Circulation.LibFLDataProviderAPI.GetLoginTypeRequest request) {
            return base.Channel.GetLoginType(request);
        }
        
        public string GetLoginType(string login) {
            Circulation.LibFLDataProviderAPI.GetLoginTypeRequest inValue = new Circulation.LibFLDataProviderAPI.GetLoginTypeRequest();
            inValue.Body = new Circulation.LibFLDataProviderAPI.GetLoginTypeRequestBody();
            inValue.Body.login = login;
            Circulation.LibFLDataProviderAPI.GetLoginTypeResponse retVal = ((Circulation.LibFLDataProviderAPI.ServiceSoap)(this)).GetLoginType(inValue);
            return retVal.Body.GetLoginTypeResult;
        }
    }
}
