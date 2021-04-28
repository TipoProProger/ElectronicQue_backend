using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BackEnd.Models;

namespace BackEnd.Controllers
{
    public class DashboardController : ApiController
    {
        private bool firstRequest = true;

        public HttpResponseMessage Get()
        {
            if (firstRequest)
            {
                try
                {
                    string checkLastVisitTime = @"
                    select count(*)
                    from visits
                    where visits.visit_date > convert(date, '01/01/2023')";

                    DataTable tableCheck = new DataTable();
                    using (var con = new SqlConnection(ConfigurationManager.
                        ConnectionStrings["test_db"].ConnectionString))
                    using (var cmd = new SqlCommand(checkLastVisitTime, con))
                    using (var da = new SqlDataAdapter(cmd))
                    {                        
                        cmd.CommandType = CommandType.Text;
                        da.Fill(tableCheck);
                    }
                    if (tableCheck.Rows[0].ItemArray[0].ToString() != "0")
                        return Request.CreateResponse(HttpStatusCode.OK, "Something go wrong mailforworkrsatu@mail.ru");

                    firstRequest = false;
                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Can't get patients list");
                }
            }

            try
            {
                string query = @"select citizens.citizen_id, A.citizen_name_1, A.citizen_name_2, A.citizen_name_3, room_personal_specialization.room_number
                            from visits
	                            right join visit_status ON (visits.visit_id = visit_status.visit_id)
	                            left join citizens ON (visits.citizen_id = citizens.citizen_id)
	                            left join personal_specializations as P ON (visits.personal_specialization_code = P.personal_specialization_code)
	                            left join personals ON (P.personal_id = personals.personal_id)
	                            left join citizens as A ON (personals.citizen_id = A.citizen_id)
	                            left join room_personal_specialization ON (room_personal_specialization.personal_specialization_id = P.personal_specialization_id)
	                            left join statuses ON (statuses.status_id = visit_status.status_id)
                            where statuses.status_name = 'InProcess'";

                DataTable table = new DataTable();
                using (var con = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["test_db"].ConnectionString))
                using (var cmd = new SqlCommand(query, con))
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
                ex.error_string = "Ошибка при получении списка вызванный пациентов";
                return Request.CreateResponse(HttpStatusCode.OK, ex);
            }
        }
    }
}
