using Paychex.Api.Api;

namespace Paychex.Api.Models.Common
{
    /// <summary>
    /// Company contact information
    /// </summary>
    public class Communication : BaseObject
    {
        /// <summary>
        /// type of contact. Values appear to be: PHONE, EMAIL, STREET_ADDRESS
        /// </summary>
        public ContactType Type { get; set; }
        /// <summary>
        /// specifies if a contact is business or personal. Can be null.  Values seen: PERSONAL, BUSINESS
        /// </summary>
        public ContactUsageType? UsageType { get; set; }
        /// <summary>
        /// Street address line 1. Only filled if type == STREET_ADDRESS.
        /// </summary>
        public string StreetLineOne { get; set; }
        /// <summary>
        /// Street address line 2. Only filled if type == STREET_ADDRESS.
        /// </summary>
        public string StreetLineTwo { get; set; }
        /// <summary>
        /// Street address City. Only filled if type == STREET_ADDRESS.
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// Street address zip/postal code. Only filled if type == STREET_ADDRESS.
        /// </summary>
        public string PostalCode { get; set; }
        /// <summary>
        /// Street address state. Only filled if type == STREET_ADDRESS.
        /// </summary>
        public string CountrySubdivisionCode { get; set; }
        /// <summary>
        /// Street address country. Only filled if type == STREET_ADDRESS.
        /// </summary>
        public string CountryCode { get; set; }
        /// <summary>
        /// Paychex internal primary key id for this contact
        /// </summary>
        public string CommunicationId {get;set;}
        /// <summary>
        /// post office boxes
        /// </summary>
        public string PostOfficeBox {get;set;}
        /// <summary>
        /// Copy from another communication object to this one. Used for random data generation.
        /// </summary>
        /// <param name="other"></param>
        protected void CopyFrom(Communication other)
        {
            City = other.City;
            CommunicationId = other.CommunicationId;
            CountryCode = other.CountryCode;
            CountrySubdivisionCode = other.CountrySubdivisionCode;
            PostalCode = other.PostalCode;
            StreetLineOne = other.StreetLineOne;
            StreetLineTwo = other.StreetLineTwo;
            Type = other.Type;
            UsageType = other.UsageType;
        }
        /// <summary>
        /// Debug info
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            if (UsageType.HasValue)
                sb.AppendFormat("{0} - ", UsageType.GetDescription());
            switch (Type)
            {
                case ContactType.STREET_ADDRESS:
                    sb.AppendFormat("{0} ", StreetLineOne);
                    if (!string.IsNullOrEmpty(StreetLineTwo))
                        sb.AppendFormat("{0} ", StreetLineTwo);
                    break;
                case ContactType.PO_BOX_ADDRESS:
                    sb.AppendFormat("{0} ", PostOfficeBox);
                    break;
                default:
                    sb.AppendFormat("id: {0}", CommunicationId);
                    return sb.ToString();
            }
            sb.Append($"{City} {CountrySubdivisionCode}, {CountryCode} - {PostalCode}");
            return sb.ToString();
        }
    }
}
