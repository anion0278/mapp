
namespace Shmap.DataAccess
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/data.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.stormware.cz/schema/version_2/data.xsd", IsNullable = false)]
    public partial class dataPack
    {

        private dataPackDataPackItem dataPackItemField;

        private decimal versionField;

        private string idField;

        private uint icoField;

        private string keyField;

        private string programVersionField;

        private string applicationField;

        private string noteField;

        /// <remarks/>
        public dataPackDataPackItem dataPackItem
        {
            get
            {
                return this.dataPackItemField;
            }
            set
            {
                this.dataPackItemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint ico
        {
            get
            {
                return this.icoField;
            }
            set
            {
                this.icoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string key
        {
            get
            {
                return this.keyField;
            }
            set
            {
                this.keyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string programVersion
        {
            get
            {
                return this.programVersionField;
            }
            set
            {
                this.programVersionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string application
        {
            get
            {
                return this.applicationField;
            }
            set
            {
                this.applicationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string note
        {
            get
            {
                return this.noteField;
            }
            set
            {
                this.noteField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/data.xsd")]
    public partial class dataPackDataPackItem
    {

        private invoice invoiceField;

        private decimal versionField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public invoice invoice
        {
            get
            {
                return this.invoiceField;
            }
            set
            {
                this.invoiceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd", IsNullable = false)]
    public partial class invoice
    {

        private invoiceInvoiceHeader invoiceHeaderField;

        private invoiceInvoiceItem[] invoiceDetailField;

        private invoiceInvoiceSummary invoiceSummaryField;

        private decimal versionField;

        /// <remarks/>
        public invoiceInvoiceHeader invoiceHeader
        {
            get
            {
                return this.invoiceHeaderField;
            }
            set
            {
                this.invoiceHeaderField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("invoiceItem", IsNullable = false)]
        public invoiceInvoiceItem[] invoiceDetail
        {
            get
            {
                return this.invoiceDetailField;
            }
            set
            {
                this.invoiceDetailField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceSummary invoiceSummary
        {
            get
            {
                return this.invoiceSummaryField;
            }
            set
            {
                this.invoiceSummaryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeader
    {

        private string invoiceTypeField;

        private invoiceInvoiceHeaderNumber numberField;

        private uint symVarField;

        private System.DateTime dateField;

        private System.DateTime dateTaxField;

        private System.DateTime dateAccountingField;

        private System.DateTime dateDueField;

        private invoiceInvoiceHeaderAccounting accountingField;

        private invoiceInvoiceHeaderClassificationVAT classificationVATField;

        private string textField;

        private invoiceInvoiceHeaderPartnerIdentity partnerIdentityField;

        private invoiceInvoiceHeaderMyIdentity myIdentityField;

        private invoiceInvoiceHeaderPaymentType paymentTypeField;

        private invoiceInvoiceHeaderAccount accountField;

        private ushort symConstField;

        private invoiceInvoiceHeaderCentre centreField;

        private invoiceInvoiceHeaderMOSS mOSSField;

        private invoiceInvoiceHeaderEvidentiaryResourcesMOSS evidentiaryResourcesMOSSField;

        private invoiceInvoiceHeaderLiquidation liquidationField;

        private bool histRateField;

        private bool markRecordField;

        /// <remarks/>
        public string invoiceType
        {
            get
            {
                return this.invoiceTypeField;
            }
            set
            {
                this.invoiceTypeField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderNumber number
        {
            get
            {
                return this.numberField;
            }
            set
            {
                this.numberField = value;
            }
        }

        /// <remarks/>
        public uint symVar
        {
            get
            {
                return this.symVarField;
            }
            set
            {
                this.symVarField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime date
        {
            get
            {
                return this.dateField;
            }
            set
            {
                this.dateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime dateTax
        {
            get
            {
                return this.dateTaxField;
            }
            set
            {
                this.dateTaxField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime dateAccounting
        {
            get
            {
                return this.dateAccountingField;
            }
            set
            {
                this.dateAccountingField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime dateDue
        {
            get
            {
                return this.dateDueField;
            }
            set
            {
                this.dateDueField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderAccounting accounting
        {
            get
            {
                return this.accountingField;
            }
            set
            {
                this.accountingField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderClassificationVAT classificationVAT
        {
            get
            {
                return this.classificationVATField;
            }
            set
            {
                this.classificationVATField = value;
            }
        }

        /// <remarks/>
        public string text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderPartnerIdentity partnerIdentity
        {
            get
            {
                return this.partnerIdentityField;
            }
            set
            {
                this.partnerIdentityField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderMyIdentity myIdentity
        {
            get
            {
                return this.myIdentityField;
            }
            set
            {
                this.myIdentityField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderPaymentType paymentType
        {
            get
            {
                return this.paymentTypeField;
            }
            set
            {
                this.paymentTypeField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderAccount account
        {
            get
            {
                return this.accountField;
            }
            set
            {
                this.accountField = value;
            }
        }

        /// <remarks/>
        public ushort symConst
        {
            get
            {
                return this.symConstField;
            }
            set
            {
                this.symConstField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderCentre centre
        {
            get
            {
                return this.centreField;
            }
            set
            {
                this.centreField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderMOSS MOSS
        {
            get
            {
                return this.mOSSField;
            }
            set
            {
                this.mOSSField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderEvidentiaryResourcesMOSS evidentiaryResourcesMOSS
        {
            get
            {
                return this.evidentiaryResourcesMOSSField;
            }
            set
            {
                this.evidentiaryResourcesMOSSField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceHeaderLiquidation liquidation
        {
            get
            {
                return this.liquidationField;
            }
            set
            {
                this.liquidationField = value;
            }
        }

        /// <remarks/>
        public bool histRate
        {
            get
            {
                return this.histRateField;
            }
            set
            {
                this.histRateField = value;
            }
        }

        /// <remarks/>
        public bool markRecord
        {
            get
            {
                return this.markRecordField;
            }
            set
            {
                this.markRecordField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderNumber
    {

        private uint numberRequestedField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public uint numberRequested
        {
            get
            {
                return this.numberRequestedField;
            }
            set
            {
                this.numberRequestedField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderAccounting
    {

        private string idsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string ids
        {
            get
            {
                return this.idsField;
            }
            set
            {
                this.idsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderClassificationVAT
    {

        private string idsField;

        private string classificationVATTypeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string ids
        {
            get
            {
                return this.idsField;
            }
            set
            {
                this.idsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string classificationVATType
        {
            get
            {
                return this.classificationVATTypeField;
            }
            set
            {
                this.classificationVATTypeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderPartnerIdentity
    {

        private address addressField;

        private shipToAddress shipToAddressField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public address address
        {
            get
            {
                return this.addressField;
            }
            set
            {
                this.addressField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public shipToAddress shipToAddress
        {
            get
            {
                return this.shipToAddressField;
            }
            set
            {
                this.shipToAddressField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd", IsNullable = false)]
    public partial class address
    {

        private string companyField;

        private string titleField;

        private string surnameField;

        private string nameField;

        private string cityField;

        private string streetField;

        private string numberField;

        private string zipField;

        private uint icoField;

        private bool icoFieldSpecified;

        private string dicField;

        private addressCountry countryField;

        private uint phoneField;

        private uint mobilPhoneField;

        private bool mobilPhoneFieldSpecified;

        private string emailField;

        /// <remarks/>
        public string company
        {
            get
            {
                return this.companyField;
            }
            set
            {
                this.companyField = value;
            }
        }

        /// <remarks/>
        public string title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        public string surname
        {
            get
            {
                return this.surnameField;
            }
            set
            {
                this.surnameField = value;
            }
        }

        /// <remarks/>
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public string city
        {
            get
            {
                return this.cityField;
            }
            set
            {
                this.cityField = value;
            }
        }

        /// <remarks/>
        public string street
        {
            get
            {
                return this.streetField;
            }
            set
            {
                this.streetField = value;
            }
        }

        /// <remarks/>
        public string number
        {
            get
            {
                return this.numberField;
            }
            set
            {
                this.numberField = value;
            }
        }

        /// <remarks/>
        public string zip
        {
            get
            {
                return this.zipField;
            }
            set
            {
                this.zipField = value;
            }
        }

        /// <remarks/>
        public uint ico
        {
            get
            {
                return this.icoField;
            }
            set
            {
                this.icoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool icoSpecified
        {
            get
            {
                return this.icoFieldSpecified;
            }
            set
            {
                this.icoFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string dic
        {
            get
            {
                return this.dicField;
            }
            set
            {
                this.dicField = value;
            }
        }

        /// <remarks/>
        public addressCountry country
        {
            get
            {
                return this.countryField;
            }
            set
            {
                this.countryField = value;
            }
        }

        /// <remarks/>
        public uint phone
        {
            get
            {
                return this.phoneField;
            }
            set
            {
                this.phoneField = value;
            }
        }

        /// <remarks/>
        public uint mobilPhone
        {
            get
            {
                return this.mobilPhoneField;
            }
            set
            {
                this.mobilPhoneField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool mobilPhoneSpecified
        {
            get
            {
                return this.mobilPhoneFieldSpecified;
            }
            set
            {
                this.mobilPhoneFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string email
        {
            get
            {
                return this.emailField;
            }
            set
            {
                this.emailField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
    public partial class addressCountry
    {

        private string idsField;

        /// <remarks/>
        public string ids
        {
            get
            {
                return this.idsField;
            }
            set
            {
                this.idsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd", IsNullable = false)]
    public partial class shipToAddress
    {

        private object nameField;

        private object cityField;

        private object streetField;

        private object phoneField;

        private object emailField;

        /// <remarks/>
        public object name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public object city
        {
            get
            {
                return this.cityField;
            }
            set
            {
                this.cityField = value;
            }
        }

        /// <remarks/>
        public object street
        {
            get
            {
                return this.streetField;
            }
            set
            {
                this.streetField = value;
            }
        }

        /// <remarks/>
        public object phone
        {
            get
            {
                return this.phoneField;
            }
            set
            {
                this.phoneField = value;
            }
        }

        /// <remarks/>
        public object email
        {
            get
            {
                return this.emailField;
            }
            set
            {
                this.emailField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderMyIdentity
    {

        private address addressField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public address address
        {
            get
            {
                return this.addressField;
            }
            set
            {
                this.addressField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderPaymentType
    {

        private string idsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string ids
        {
            get
            {
                return this.idsField;
            }
            set
            {
                this.idsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderAccount
    {

        private string idsField;

        private uint accountNoField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string ids
        {
            get
            {
                return this.idsField;
            }
            set
            {
                this.idsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public uint accountNo
        {
            get
            {
                return this.accountNoField;
            }
            set
            {
                this.accountNoField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderCentre
    {

        private string idsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string ids
        {
            get
            {
                return this.idsField;
            }
            set
            {
                this.idsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderMOSS
    {

        private string idsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string ids
        {
            get
            {
                return this.idsField;
            }
            set
            {
                this.idsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderEvidentiaryResourcesMOSS
    {

        private string idsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public string ids
        {
            get
            {
                return this.idsField;
            }
            set
            {
                this.idsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceHeaderLiquidation
    {

        private decimal amountHomeField;

        private byte amountForeignField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal amountHome
        {
            get
            {
                return this.amountHomeField;
            }
            set
            {
                this.amountHomeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public byte amountForeign
        {
            get
            {
                return this.amountForeignField;
            }
            set
            {
                this.amountForeignField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceItem
    {

        private string textField;

        private decimal quantityField;

        private string unitField;

        private decimal coefficientField;

        private bool payVATField;

        private string rateVATField;

        private decimal percentVATField;

        private decimal discountPercentageField;

        private invoiceInvoiceItemHomeCurrency homeCurrencyField;

        private invoiceInvoiceItemForeignCurrency foreignCurrencyField;

        private string noteField;

        private byte codeField;

        private bool codeFieldSpecified;

        private invoiceInvoiceItemStockItem stockItemField;

        private bool pDPField;

        /// <remarks/>
        public string text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>
        public decimal quantity
        {
            get
            {
                return this.quantityField;
            }
            set
            {
                this.quantityField = value;
            }
        }

        /// <remarks/>
        public string unit
        {
            get
            {
                return this.unitField;
            }
            set
            {
                this.unitField = value;
            }
        }

        /// <remarks/>
        public decimal coefficient
        {
            get
            {
                return this.coefficientField;
            }
            set
            {
                this.coefficientField = value;
            }
        }

        /// <remarks/>
        public bool payVAT
        {
            get
            {
                return this.payVATField;
            }
            set
            {
                this.payVATField = value;
            }
        }

        /// <remarks/>
        public string rateVAT
        {
            get
            {
                return this.rateVATField;
            }
            set
            {
                this.rateVATField = value;
            }
        }

        /// <remarks/>
        public decimal percentVAT
        {
            get
            {
                return this.percentVATField;
            }
            set
            {
                this.percentVATField = value;
            }
        }

        /// <remarks/>
        public decimal discountPercentage
        {
            get
            {
                return this.discountPercentageField;
            }
            set
            {
                this.discountPercentageField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceItemHomeCurrency homeCurrency
        {
            get
            {
                return this.homeCurrencyField;
            }
            set
            {
                this.homeCurrencyField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceItemForeignCurrency foreignCurrency
        {
            get
            {
                return this.foreignCurrencyField;
            }
            set
            {
                this.foreignCurrencyField = value;
            }
        }

        /// <remarks/>
        public string note
        {
            get
            {
                return this.noteField;
            }
            set
            {
                this.noteField = value;
            }
        }

        /// <remarks/>
        public byte code
        {
            get
            {
                return this.codeField;
            }
            set
            {
                this.codeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool codeSpecified
        {
            get
            {
                return this.codeFieldSpecified;
            }
            set
            {
                this.codeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceItemStockItem stockItem
        {
            get
            {
                return this.stockItemField;
            }
            set
            {
                this.stockItemField = value;
            }
        }

        /// <remarks/>
        public bool PDP
        {
            get
            {
                return this.pDPField;
            }
            set
            {
                this.pDPField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceItemHomeCurrency
    {

        private decimal unitPriceField;

        private decimal priceField;

        private decimal priceVATField;

        private decimal priceSumField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal unitPrice
        {
            get
            {
                return this.unitPriceField;
            }
            set
            {
                this.unitPriceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal price
        {
            get
            {
                return this.priceField;
            }
            set
            {
                this.priceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal priceVAT
        {
            get
            {
                return this.priceVATField;
            }
            set
            {
                this.priceVATField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal priceSum
        {
            get
            {
                return this.priceSumField;
            }
            set
            {
                this.priceSumField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceItemForeignCurrency
    {

        private decimal unitPriceField;

        private decimal priceField;

        private decimal priceVATField;

        private decimal priceSumField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal unitPrice
        {
            get
            {
                return this.unitPriceField;
            }
            set
            {
                this.unitPriceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal price
        {
            get
            {
                return this.priceField;
            }
            set
            {
                this.priceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal priceVAT
        {
            get
            {
                return this.priceVATField;
            }
            set
            {
                this.priceVATField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal priceSum
        {
            get
            {
                return this.priceSumField;
            }
            set
            {
                this.priceSumField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceItemStockItem
    {

        private store storeField;

        private stockItem stockItemField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public store store
        {
            get
            {
                return this.storeField;
            }
            set
            {
                this.storeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public stockItem stockItem
        {
            get
            {
                return this.stockItemField;
            }
            set
            {
                this.stockItemField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd", IsNullable = false)]
    public partial class store
    {

        private string idsField;

        /// <remarks/>
        public string ids
        {
            get
            {
                return this.idsField;
            }
            set
            {
                this.idsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd", IsNullable = false)]
    public partial class stockItem
    {

        private byte idsField;

        private ulong eANField;

        /// <remarks/>
        public byte ids
        {
            get
            {
                return this.idsField;
            }
            set
            {
                this.idsField = value;
            }
        }

        /// <remarks/>
        public ulong EAN
        {
            get
            {
                return this.eANField;
            }
            set
            {
                this.eANField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceSummary
    {

        private string roundingDocumentField;

        private string roundingVATField;

        private string typeCalculateVATInclusivePriceField;

        private invoiceInvoiceSummaryHomeCurrency homeCurrencyField;

        private invoiceInvoiceSummaryForeignCurrency foreignCurrencyField;

        /// <remarks/>
        public string roundingDocument
        {
            get
            {
                return this.roundingDocumentField;
            }
            set
            {
                this.roundingDocumentField = value;
            }
        }

        /// <remarks/>
        public string roundingVAT
        {
            get
            {
                return this.roundingVATField;
            }
            set
            {
                this.roundingVATField = value;
            }
        }

        /// <remarks/>
        public string typeCalculateVATInclusivePrice
        {
            get
            {
                return this.typeCalculateVATInclusivePriceField;
            }
            set
            {
                this.typeCalculateVATInclusivePriceField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceSummaryHomeCurrency homeCurrency
        {
            get
            {
                return this.homeCurrencyField;
            }
            set
            {
                this.homeCurrencyField = value;
            }
        }

        /// <remarks/>
        public invoiceInvoiceSummaryForeignCurrency foreignCurrency
        {
            get
            {
                return this.foreignCurrencyField;
            }
            set
            {
                this.foreignCurrencyField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceSummaryHomeCurrency
    {

        private byte priceNoneField;

        private byte priceLowField;

        private byte priceLowVATField;

        private byte priceLowSumField;

        private decimal priceHighField;

        private decimal priceHighVATField;

        private decimal priceHighSumField;

        private byte price3Field;

        private byte price3VATField;

        private byte price3SumField;

        private round roundField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public byte priceNone
        {
            get
            {
                return this.priceNoneField;
            }
            set
            {
                this.priceNoneField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public byte priceLow
        {
            get
            {
                return this.priceLowField;
            }
            set
            {
                this.priceLowField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public byte priceLowVAT
        {
            get
            {
                return this.priceLowVATField;
            }
            set
            {
                this.priceLowVATField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public byte priceLowSum
        {
            get
            {
                return this.priceLowSumField;
            }
            set
            {
                this.priceLowSumField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal priceHigh
        {
            get
            {
                return this.priceHighField;
            }
            set
            {
                this.priceHighField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal priceHighVAT
        {
            get
            {
                return this.priceHighVATField;
            }
            set
            {
                this.priceHighVATField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal priceHighSum
        {
            get
            {
                return this.priceHighSumField;
            }
            set
            {
                this.priceHighSumField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public byte price3
        {
            get
            {
                return this.price3Field;
            }
            set
            {
                this.price3Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public byte price3VAT
        {
            get
            {
                return this.price3VATField;
            }
            set
            {
                this.price3VATField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public byte price3Sum
        {
            get
            {
                return this.price3SumField;
            }
            set
            {
                this.price3SumField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public round round
        {
            get
            {
                return this.roundField;
            }
            set
            {
                this.roundField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd", IsNullable = false)]
    public partial class round
    {

        private byte priceRoundField;

        /// <remarks/>
        public byte priceRound
        {
            get
            {
                return this.priceRoundField;
            }
            set
            {
                this.priceRoundField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
    public partial class invoiceInvoiceSummaryForeignCurrency
    {

        private currency currencyField;

        private decimal rateField;

        private byte amountField;

        private decimal priceSumField;

        private round roundField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public currency currency
        {
            get
            {
                return this.currencyField;
            }
            set
            {
                this.currencyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal rate
        {
            get
            {
                return this.rateField;
            }
            set
            {
                this.rateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public byte amount
        {
            get
            {
                return this.amountField;
            }
            set
            {
                this.amountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public decimal priceSum
        {
            get
            {
                return this.priceSumField;
            }
            set
            {
                this.priceSumField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public round round
        {
            get
            {
                return this.roundField;
            }
            set
            {
                this.roundField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd", IsNullable = false)]
    public partial class currency
    {

        private string idsField;

        /// <remarks/>
        public string ids
        {
            get
            {
                return this.idsField;
            }
            set
            {
                this.idsField = value;
            }
        }
    }
}