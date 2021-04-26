using BackEnd.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BackEnd.Controllers
{
    public class DoctorsController : ApiController
    {
        public HttpResponseMessage Get()
        {
            try
            {
                string querry = @"
                select citizen_name_1, citizen_name_2, citizen_name_3, specialization_name, personal_specializations.personal_specialization_id, room_personal_specialization.room_number
                from citizens
                right join personals ON (citizens.citizen_id = personals.citizen_id)
                left join personal_specializations ON (personals.personal_id = personal_specializations.personal_id)
                left join specializations ON (specializations.specialization_id = personal_specializations.specialization_id)
                left join room_personal_specialization ON(room_personal_specialization.personal_specialization_id = personal_specializations.personal_specialization_id)
                where company_id = 44849";

                DataTable table = new DataTable();
                using (var con = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["test_db"].ConnectionString))
                using (var cmd = new SqlCommand(querry, con))
                using (var da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.Text;
                    da.Fill(table);
                }

                return Request.CreateResponse(HttpStatusCode.OK, table);
            }
            catch(Exception)
            {
                custom_exception ex = new custom_exception();
                ex.error_string = "Ошибка при создании списка врачей";
                return Request.CreateResponse(HttpStatusCode.OK, ex);
            }
        }

        // POST api/doctors (for find doctor)
        public HttpResponseMessage Post(citizens citizen)
        {
            try
            {
                string query = @"
                select citizen_name_1, citizen_name_2, citizen_name_3, specialization_name, personal_specializations.personal_specialization_id, room_personal_specialization.room_number
                from citizens
                right join personals ON (citizens.citizen_id = personals.citizen_id)
                left join personal_specializations ON (personals.personal_id = personal_specializations.personal_id)
                left join specializations ON (specializations.specialization_id = personal_specializations.specialization_id)
                left join room_personal_specialization ON (room_personal_specialization.personal_specialization_id = personal_specializations.personal_specialization_id)
                where company_id = 44849
                    and citizens.citizen_name_1 like @name_1
                    and citizens.citizen_name_2 like @name_2
                    and citizens.citizen_name_3 like @name_3";
                DataTable table = new DataTable();
                using (var con = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["test_db"].ConnectionString))
                using (var cmd = new SqlCommand(query, con))
                using (var da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.Text;
                    var param1 = cmd.Parameters.Add("@name_1", SqlDbType.VarChar, 30);
                    param1.Value = citizen.citizen_name_1 + "%";
                    var param2 = cmd.Parameters.Add("@name_2", SqlDbType.VarChar, 30);
                    param2.Value = citizen.citizen_name_2 + "%";
                    var param3 = cmd.Parameters.Add("@name_3", SqlDbType.VarChar, 30);
                    param3.Value = citizen.citizen_name_3 + "%";
                    da.Fill(table);
                }

                return Request.CreateResponse(HttpStatusCode.OK, table);
            }
            catch(Exception)
            {
                custom_exception ex = new custom_exception();
                ex.error_string = "Ошибка при поиске врача по ФИО";
                return Request.CreateResponse(HttpStatusCode.OK, ex);
            }
        }

        // PUT api/doctors (set doctors room)
        public HttpResponseMessage Put(room_personal_specialization room)
        {
            try
            {
                string query = @"update room_personal_specialization 
                    set personal_specialization_id = @personalSpecializationId
                    where room_number = @roomNumber";
                DataTable table = new DataTable();
                using (var con = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["test_db"].ConnectionString))
                using (var cmd = new SqlCommand(query, con))
                using (var da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.Text;
                    var param1 = cmd.Parameters.Add("@roomNumber", SqlDbType.VarChar, 10);
                    param1.Value = room.room_number;
                    var param2 = cmd.Parameters.Add("@personalSpecializationId", SqlDbType.Int);
                    param2.Value = room.personal_specialization_id;
                    da.Fill(table);
                }

                return Request.CreateResponse(HttpStatusCode.OK, "Success in update room");
            }
            catch (Exception)
            {
                custom_exception ex = new custom_exception();
                ex.error_string = "Ошибка при поиске врача по ФИО";
                return Request.CreateResponse(HttpStatusCode.OK, ex);
            }
        }
    }
}
