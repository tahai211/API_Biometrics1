using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace MockUp_CardZ.Controllers
{
    [Route("CARDAPI/[controller]")]
    [ApiController]
    public class CardController : ControllerBase
    {
        private readonly DatabaseHelper databaseHelper;

        public CardController()
        {
            // Thay đổi chuỗi kết nối tùy thuộc vào cơ sở dữ liệu của bạn
            string connectionString = "Data Source=172.26.28.241;Initial Catalog=IDPMOCK;User Id=idp;Password=idp;";
            databaseHelper = new DatabaseHelper(connectionString);
        }
       
        public class card
        {
            public string cifNo { get; set; } = "";
            public string cardNo { get; set; } = string.Empty;
            public string cardPlasticCode { get; set; } = string.Empty;
        }
        public class GLcard
        {
            public string CusID { get; set; } = "";
            public string CardNo { get; set; } = string.Empty;
            public decimal Amount { get; set; } = 0;

            public string CCYID { get; set; } = string.Empty;
            public string DescTranCode { get; set; } = string.Empty;
        }
        public card ParseXml( string xmlString)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(card));

                using (XmlReader reader = XmlReader.Create(new System.IO.StringReader(xmlString)))
                {
                    card student = (card)serializer.Deserialize(reader);
                    return student;
                }
            }
            catch (Exception ex)
            {
                return new card(); // Xử lý lỗi nếu có
            }
        }
        [HttpPost("CardList")]
        public List<Card> GetCards([FromBody] card customer)
        {
            //card customer = ParseXml(xmlString);
            string cusId = customer.cifNo;
            var cardList = databaseHelper.GetCards(cusId);
            return cardList;
        }
        [HttpPost("CardInfo")]
        public Card GetCardInfo([FromBody] card card)
        {
            string cardNo = card.cardNo;
            var cardInfo = databaseHelper.GetCardInfo(cardNo);
            return cardInfo;
        }
        [HttpPost("CardListByCusId")]
        public List<Card> GetCardListByCustId([FromBody] card card)
        {
            string cusId = card.cifNo;
            string cardNo = card.cardNo;
            var cardInfo = databaseHelper.GetCardListByCustId(cusId, cardNo);
            return cardInfo;
        }
        [HttpPost("UpdateCardStatus")]
        public object UpdateCardStatus([FromBody] card card)
        {
            string cardPlasticCode = card.cardPlasticCode;
            string cardNo = card.cardNo;
            var cardInfo = databaseHelper.UpdateCardStatus(cardPlasticCode, cardNo);
            return cardInfo;
        }
        [HttpPost("FinanceAdj")]
        public bool FinanceAdj([FromBody] GLcard card)
        {
            
            return true;
        }
    }
}
