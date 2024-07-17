using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockUp_CardZ
{
    public class Card
    {
        public string CusId { get; set; }
        public string CardNo { get; set; }
        public string CardType { get; set; }
        public string CCYID { get; set; }
        public string CardHolderName { get; set; }
        public decimal CreditLimit { get; set; }

        public decimal DebitLimit { get; set; }
        public decimal AvaiLimit { get; set; }

        public string BasicCardNo { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime DueDate { get; set; }

        public string Status { get; set; }

    }
    public class DatabaseHelper
    {
        private readonly string connectionString;

        public DatabaseHelper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<Card> GetCards(string cusId)
        {
            List<Card> cards = new List<Card>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Idp_CardbyCus WHERE CusId = @CusId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CusId", cusId);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Card card = new Card
                                {
                                    CusId = reader.GetString(0),
                                    CardNo = reader.GetString(1),
                                    CardType = reader.GetString(2),
                                    CCYID = reader.GetString(3),
                                    CardHolderName = reader.GetString(4),
                                    CreditLimit = reader.GetDecimal(5),
                                    DebitLimit = reader.GetDecimal(6),
                                    AvaiLimit = reader.GetDecimal(7),
                                    BasicCardNo = reader.GetString(8),
                                    DateCreate = reader.GetDateTime(9),
                                    DueDate = reader.GetDateTime(10),
                                    Status = reader.GetString(11)
                                };
                                cards.Add(card);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu cần thiết, ví dụ: ghi log, thông báo lỗi, v.v...
                // Đảm bảo rằng bạn không để trống catch block mà xử lý ngoại lệ một cách hợp lý.
            }

            return cards;
        }
        public List<Card> GetCardListByCustId(string cusId, string cardNo)
        {
            List<Card> employees = new List<Card>();
            try
            {

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Idp_CardbyCus WHERE CusId = @CusId AND CardNo = @CardNo";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CusId", cusId);
                        command.Parameters.AddWithValue("@CardNo", cardNo);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Card employee = new Card
                                {
                                    CusId = reader.GetString(0),
                                    CardNo = reader.GetString(1),
                                    CardType = reader.GetString(2),
                                    CCYID = reader.GetString(3),
                                    CardHolderName = reader.GetString(4),
                                    CreditLimit = reader.GetDecimal(5),
                                    DebitLimit = reader.GetDecimal(6),
                                    AvaiLimit = reader.GetDecimal(7),
                                    BasicCardNo = reader.GetString(8),
                                    DateCreate = reader.GetDateTime(9),
                                    DueDate = reader.GetDateTime(10),
                                    Status = reader.GetString(11)
                                };
                                employees.Add(employee);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }

            return employees;
        }
        public Card GetCardInfo(string cardNo)
        {

                Card card = new Card();
            try
            {

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Idp_CardbyCus WHERE  CardNo = @CardNo";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CardNo", cardNo);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                card = new Card
                                {
                                    CusId = reader.GetString(0),
                                    CardNo = reader.GetString(1),
                                    CardType = reader.GetString(2),
                                    CCYID = reader.GetString(3),
                                    CardHolderName = reader.GetString(4),
                                    CreditLimit = reader.GetDecimal(5),
                                    DebitLimit = reader.GetDecimal(6),
                                    AvaiLimit = reader.GetDecimal(7),
                                    BasicCardNo = reader.GetString(8),
                                    DateCreate = reader.GetDateTime(9),
                                    DueDate = reader.GetDateTime(10),
                                    Status = reader.GetString(11),
                                };
                            }
                        }
                    }
                }
            }catch(Exception ex)
            {

            }

                return card;
            }
        public object UpdateCardStatus(string CardPlasticCode, string CardNo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Idp_CardbyCus SET [Status] = @CardPlasticCode WHERE CardNo = @CardNo";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CardNo", CardNo);
                        command.Parameters.AddWithValue("@CardPlasticCode", CardPlasticCode);
                        connection.Open();
                        command.ExecuteNonQuery(); // Execute the SQL update statement
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the update process
            }

            // Return the updated CardNo and CardPlasticCode as an anonymous object
            return new { cardNo = CardNo, cardPlasticCode = CardPlasticCode };
        }

    }
}
