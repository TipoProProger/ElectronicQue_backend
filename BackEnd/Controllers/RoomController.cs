using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Data;
using System.Net.Http;
using System.Web.Http;
using BackEnd.Models;
using System.Data.SqlClient;
using System.Configuration;

namespace BackEnd.Controllers
{
    public class RoomController : ApiController
    {     
        public HttpResponseMessage Get(int id)
        {
            try
            {
                string query = @"
                select top 1 room_personal_specialization.room_id, room_personal_specialization.room_number
                from room_personal_specialization
                left join personal_specializations ON (room_personal_specialization.personal_specialization_id = personal_specializations.personal_specialization_id)
                where personal_specializations.personal_specialization_id = @personalSpecializationId
                ";
                DataTable table = new DataTable();
                using (var con = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["test_db"].ConnectionString))
                using (var cmd = new SqlCommand(query, con))
                using (var da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.Text;
                    var param1 = cmd.Parameters.Add("@personalSpecializationId", SqlDbType.VarChar, 30);
                    param1.Value = id;
                    da.Fill(table);
                }

                return Request.CreateResponse(HttpStatusCode.OK, table);
            }
            catch(Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Can't get room for personal");
            }
            
        }
        
        public HttpResponseMessage Post(room_personal_specialization room)
        {
            try
            {
                string query = @"
                    update room_personal_specialization set room_number = @roomNumber
                    where room_personal_specialization.personal_specialization_id = @personalSpecializationId
                ";
                DataTable table = new DataTable();
                using (var con = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["test_db"].ConnectionString))
                using (var cmd = new SqlCommand(query, con))
                using (var da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.Text;
                    var param1 = cmd.Parameters.Add("@personalSpecializationId", SqlDbType.Int);
                    param1.Value = room.personal_specialization_id;
                    var param2 = cmd.Parameters.Add("@roomNumber", SqlDbType.VarChar, 10);
                    param2.Value = room.room_number;
                    da.Fill(table);
                }

                return Request.CreateResponse(HttpStatusCode.OK, table);
            }
            catch (Exception)
            {
                custom_exception ex = new custom_exception();
                ex.error_string = "Ошибка при задании новой комнаты";
                return Request.CreateResponse(HttpStatusCode.OK, ex);
            }
        }
    }
}
