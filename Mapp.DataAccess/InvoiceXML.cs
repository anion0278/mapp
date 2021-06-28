using System;
using System.Xml;
using System.Xml.Serialization;

namespace Mapp
{
    public class InvoiceXml
    {
        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/data.xsd")]
        [XmlRoot(IsNullable = false, Namespace = "http://www.stormware.cz/schema/version_2/data.xsd")]
        public class dataPack
        {
            private InvoiceXml.dataPackDataPackItem[] dataPackItemField;
            private decimal versionField;
            private string idField;
            private uint icoField;
            private string keyField;
            private string programVersionField;
            private string applicationField;
            private string noteField;

            [XmlElement("dataPackItem")]
            public InvoiceXml.dataPackDataPackItem[] dataPackItem
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

            [XmlAttribute]
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

            [XmlAttribute]
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

            [XmlAttribute]
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

            [XmlAttribute]
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

            [XmlAttribute]
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

            [XmlAttribute]
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

            [XmlAttribute]
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/data.xsd")]
        public class dataPackDataPackItem
        {
            private InvoiceXml.invoice invoiceField;
            private decimal versionField;
            private string idField;

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
            public InvoiceXml.invoice invoice
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

            [XmlAttribute]
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

            [XmlAttribute]
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        [XmlRoot(IsNullable = false, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public class invoice
        {
            [XmlNamespaceDeclarations]
            public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces(new XmlQualifiedName[1]
            {
        new XmlQualifiedName("inv")
            });
            private InvoiceXml.invoiceInvoiceHeader invoiceHeaderField;
            private InvoiceXml.invoiceInvoiceItem[] invoiceDetailField;
            private InvoiceXml.invoiceInvoiceSummary invoiceSummaryField;
            private decimal versionField;

            public InvoiceXml.invoiceInvoiceHeader invoiceHeader
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

            [XmlArrayItem("invoiceItem", IsNullable = false)]
            public InvoiceXml.invoiceInvoiceItem[] invoiceDetail
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

            public InvoiceXml.invoiceInvoiceSummary invoiceSummary
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

            [XmlAttribute]
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public class invoiceInvoiceHeader
        {
            [XmlNamespaceDeclarations]
            public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces(new XmlQualifiedName[1]
            {
        new XmlQualifiedName("typ", "http://www.stormware.cz/schema/version_2/type.xsd")
            });
            private string invoiceTypeField;
            private InvoiceXml.invoiceInvoiceHeaderNumber numberField;
            private string symVarField;
            private string symParField;
            private DateTime dateField;
            private DateTime dateTaxField;
            private DateTime dateAccountingField;
            private DateTime dateDueField;
            private InvoiceXml.invoiceInvoiceHeaderAccounting accountingField;
            private InvoiceXml.invoiceInvoiceHeaderClassificationVAT classificationVATField;
            private string textField;
            private InvoiceXml.invoiceInvoiceHeaderPartnerIdentity partnerIdentityField;
            private InvoiceXml.invoiceInvoiceHeaderMyIdentity myIdentityField;
            private InvoiceXml.invoiceInvoiceHeaderPaymentType paymentTypeField;
            private InvoiceXml.invoiceInvoiceHeaderAccount accountField;
            private ushort symConstField;
            private InvoiceXml.invoiceInvoiceHeaderCentre centreField;
            private invoiceInvoiceHeaderCarrier carrierField;
            private InvoiceXml.invoiceInvoiceHeaderLiquidation liquidationField;
            private bool markRecordField;
            private invoiceInvoiceHeaderMOSS mOSSField;
            private invoiceInvoiceHeaderEvidentiaryResourcesMOSS evidentiaryResourcesMOSSField;
            private bool histRateField;


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

            public InvoiceXml.invoiceInvoiceHeaderNumber number
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

            public string symVar
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

            public string symPar
            {
                get
                {
                    return this.symParField;
                }
                set
                {
                    this.symParField = value;
                }
            }

            [XmlElement(DataType = "date")]
            public DateTime date
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

            [XmlElement(DataType = "date")]
            public DateTime dateTax
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

            [XmlElement(DataType = "date")]
            public DateTime dateAccounting
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

            [XmlElement(DataType = "date")]
            public DateTime dateDue
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

            public InvoiceXml.invoiceInvoiceHeaderAccounting accounting
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

            public InvoiceXml.invoiceInvoiceHeaderClassificationVAT classificationVAT
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

            public InvoiceXml.invoiceInvoiceHeaderPartnerIdentity partnerIdentity
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

            public InvoiceXml.invoiceInvoiceHeaderMyIdentity myIdentity
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

            public InvoiceXml.invoiceInvoiceHeaderPaymentType paymentType
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

            public InvoiceXml.invoiceInvoiceHeaderAccount account
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

            public InvoiceXml.invoiceInvoiceHeaderCentre centre
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

            public invoiceInvoiceHeaderCarrier carrier
            {
                get
                {
                    return this.carrierField;
                }
                set
                {
                    this.carrierField = value;
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

            public InvoiceXml.invoiceInvoiceHeaderLiquidation liquidation
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public class invoiceInvoiceHeaderNumber
        {
            private uint numberRequestedField;

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public class invoiceInvoiceHeaderAccounting
        {
            private string idsField;

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public class invoiceInvoiceHeaderClassificationVAT
        {
            private string idsField;
            private string classificationVATTypeField;

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public class invoiceInvoiceHeaderPartnerIdentity
        {
            private InvoiceXml.address addressField;
            private InvoiceXml.shipToAddress shipToAddressField;

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
            public InvoiceXml.address address
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
            public InvoiceXml.shipToAddress shipToAddress
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        [XmlRoot(IsNullable = false, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public class address
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
            private InvoiceXml.addressCountry countryField;
            private string phoneField;
            private string mobilPhoneField;
            private string emailField;

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

            [XmlIgnore]
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

            public InvoiceXml.addressCountry country
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

            public string phone
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

            public string mobilPhone
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public class addressCountry
        {
            private string idsField;

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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        [XmlRoot(IsNullable = false, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public class shipToAddress
        {
            private object nameField;
            private object cityField;
            private object streetField;
            private object phoneField;

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
        }

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public class invoiceInvoiceHeaderMyIdentity
        {
            private InvoiceXml.address addressField;

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
            public InvoiceXml.address address
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public class invoiceInvoiceHeaderPaymentType
        {
            private string idsField;

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public class invoiceInvoiceHeaderAccount
        {
            private string idsField;
            private uint accountNoField;

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public class invoiceInvoiceHeaderCentre
        {
            private string idsField;

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

        [XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public partial class invoiceInvoiceHeaderCarrier
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public class invoiceInvoiceHeaderLiquidation
        {
            private decimal amountHomeField;
            private decimal amountForeignField;

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
            public decimal amountForeign
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public class invoiceInvoiceItem
        {
            [XmlNamespaceDeclarations]
            public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces(new XmlQualifiedName[1]
            {
        new XmlQualifiedName("typ", "http://www.stormware.cz/schema/version_2/type.xsd")
            });
            private string textField;
            private decimal quantityField;
            private string unitField;
            private decimal coefficientField;
            private bool payVATField;
            private string rateVATField;
            private decimal discountPercentageField;
            private InvoiceXml.invoiceInvoiceItemHomeCurrency homeCurrencyField;
            private InvoiceXml.invoiceInvoiceItemForeignCurrency foreignCurrencyField;
            private string codeField;
            private InvoiceXml.invoiceInvoiceItemStockItem stockItemField;
            private bool pDPField;

            [XmlIgnore]
            public string amazonSkuCode { get; set; } 

            [XmlIgnore]
            public bool IsShipping { get; set; } = false;

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

            public InvoiceXml.invoiceInvoiceItemHomeCurrency homeCurrency
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

            public InvoiceXml.invoiceInvoiceItemForeignCurrency foreignCurrency
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

            public string code
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

            public InvoiceXml.invoiceInvoiceItemStockItem stockItem
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public class invoiceInvoiceItemHomeCurrency
        {
            private decimal unitPriceField;
            private decimal priceField;
            private decimal priceVATField;
            private decimal priceSumField;

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public class invoiceInvoiceItemForeignCurrency
        {
            private decimal unitPriceField;
            private decimal priceField;
            private decimal priceVATField;
            private decimal priceSumField;

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public class invoiceInvoiceItemStockItem
        {
            private InvoiceXml.store storeField;
            private InvoiceXml.stockItem stockItemField;

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
            public InvoiceXml.store store
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
            public InvoiceXml.stockItem stockItem
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        [XmlRoot(IsNullable = false, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public class store
        {
            private string idsField;

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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        [XmlRoot(IsNullable = false, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public class stockItem
        {
            private string idsField;

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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public class invoiceInvoiceSummary
        {
            [XmlNamespaceDeclarations]
            public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces(new XmlQualifiedName[1]
            {
        new XmlQualifiedName("typ", "http://www.stormware.cz/schema/version_2/type.xsd")
            });
            private string roundingDocumentField;
            private string roundingVATField;
            private InvoiceXml.invoiceInvoiceSummaryHomeCurrency homeCurrencyField;
            private InvoiceXml.invoiceInvoiceSummaryForeignCurrency foreignCurrencyField;

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

            public InvoiceXml.invoiceInvoiceSummaryHomeCurrency homeCurrency
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

            public InvoiceXml.invoiceInvoiceSummaryForeignCurrency foreignCurrency
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public class invoiceInvoiceSummaryHomeCurrency
        {
            private decimal priceNoneField;
            private decimal priceLowField;
            private decimal priceLowVATField;
            private decimal priceLowSumField;
            private decimal priceHighField;
            private decimal priceHighVATField;
            private decimal priceHighSumField;
            private decimal price3Field;
            private decimal price3VATField;
            private decimal price3SumField;
            private InvoiceXml.round roundField;

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
            public decimal priceNone
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
            public decimal priceLow
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
            public decimal priceLowVAT
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
            public decimal priceLowSum
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
            public decimal price3
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
            public decimal price3VAT
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
            public decimal price3Sum
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
            public InvoiceXml.round round
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        [XmlRoot(IsNullable = false, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public class round
        {
            private byte priceRoundField;

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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/invoice.xsd")]
        public class invoiceInvoiceSummaryForeignCurrency
        {
            private InvoiceXml.currency currencyField;
            private decimal rateField;
            private byte amountField;
            private decimal priceSumField;

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
            public InvoiceXml.currency currency
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

            [XmlElement(Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
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

        [XmlType(AnonymousType = true, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        [XmlRoot(IsNullable = false, Namespace = "http://www.stormware.cz/schema/version_2/type.xsd")]
        public class currency
        {
            private string idsField;

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
}
