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
    enum STATES : int { STATE_FIRST_WITH_TIME = 0, STATE_FIRST_WITHOUT_TIME };
    public class DoctorPatientListController : ApiController
    {
        public HttpResponseMessage Get(int id)
        {
            try
            {
                string checkLastVisitTime = @"
                    select count(*)
                    from visits
                    where visits.visit_date > convert(date, '15/07/2021')";

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
            }
            catch(Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Can't get patients list");
            }

            try
            {
                bool withTime = true;

                #region Check last patient queue status (have time or not)
                string checkLastVisitTime = @"
                select top 1 visits.visit_time
                from visit_status
                left join statuses ON (visit_status.status_id = statuses.status_id)                
				left join visits ON (visits.visit_id = visit_status.visit_id)		
				left join personal_specializations ON (personal_specializations.personal_specialization_code = visits.personal_specialization_code)
                where statuses.status_name = 'Finished'
					and personal_specializations.personal_specialization_id = @id
                order by visit_status.last_update desc";

                DataTable tableCheck = new DataTable();
                using (var con = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["test_db"].ConnectionString))
                using (var cmd = new SqlCommand(checkLastVisitTime, con))
                using (var da = new SqlDataAdapter(cmd))
                {
                    var param = cmd.Parameters.Add("@id", SqlDbType.Int);
                    param.Value = id;
                    cmd.CommandType = CommandType.Text;
                    da.Fill(tableCheck);
                }
                if (tableCheck.Rows.OfType<DataRow>().Any(r => r.ItemArray[0].ToString() == ":бв"))
                    withTime = false;
                #endregion

                #region Get data about patients with time
                string querry = @"
                select citizens.citizen_name_1, citizens.citizen_name_2, citizens.citizen_name_3, visits.citizen_id, visits.visit_date, visits.visit_time, statuses.status_name
                    from visits
                    left join citizens ON (visits.citizen_id = citizens.citizen_id)
	                left join visit_status ON (visits.visit_id = visit_status.visit_id)
	                left join statuses ON (visit_status.status_id = statuses.status_id)
                    where visits.personal_specialization_code IN (
		                select personal_specializations.personal_specialization_code
		                from personal_specializations
		                where personal_specialization_id = @personalSpecializationId)
	                and (statuses.status_name IS NULL or statuses.status_name = 'InProcess')
                    and convert(date, visits.visit_date) = convert(date, GETDATE())
                    and visits.visit_time != ':бв'
                    order by visits.visit_time";

                DataTable tableWithTime = new DataTable();
                using (var con = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["test_db"].ConnectionString))
                using (var cmd = new SqlCommand(querry, con))
                using (var da = new SqlDataAdapter(cmd))
                {
                    var param = cmd.Parameters.Add("@personalSpecializationId", SqlDbType.Int);
                    param.Value = id;
                    cmd.CommandType = CommandType.Text;
                    da.Fill(tableWithTime);
                }
                #endregion

                #region Get data about patients without time
                string queryWithoutTime = @"
                select citizens.citizen_name_1, citizens.citizen_name_2, citizens.citizen_name_3, visits.citizen_id, visits.visit_date, visits.visit_time, statuses.status_name
                    from visits
                    left join citizens ON (visits.citizen_id = citizens.citizen_id)
	                left join visit_status ON (visits.visit_id = visit_status.visit_id)
	                left join statuses ON (visit_status.status_id = statuses.status_id)
                    where visits.personal_specialization_code IN (
		                select personal_specializations.personal_specialization_code
		                from personal_specializations
		                where personal_specialization_id = @personalSpecializationId)
	                and (statuses.status_name IS NULL or (statuses.status_name != 'Finished' and statuses.status_name != 'Terminated'))
                    and convert(date, visits.visit_date) = convert(date, GETDATE())
                    and (visits.visit_time = ':бв')
                    order by visits.visit_id";

                DataTable tableWithoutTime = new DataTable();
                using (var con = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["test_db"].ConnectionString))
                using (var cmd = new SqlCommand(queryWithoutTime, con))
                using (var da = new SqlDataAdapter(cmd))
                {
                    var param = cmd.Parameters.Add("@personalSpecializationId", SqlDbType.Int);
                    param.Value = id;
                    cmd.CommandType = CommandType.Text;
                    da.Fill(tableWithoutTime);
                }
                #endregion

                #region Merge data inspecific way
                //one with time, one without time, and so on
                int tableWithTimeI = 0;
                int tableWithoutTimeI = 0;
                DataTable table = tableWithTime.Clone();

                if (withTime)
                    if (tableWithoutTimeI < tableWithoutTime.Rows.Count)
                        table.Rows.Add(tableWithoutTime.Rows[tableWithoutTimeI++].ItemArray);

                bool flag = true;
                while (flag)
                {
                    flag = false;
                    if (tableWithTimeI < tableWithTime.Rows.Count)
                    {
                        table.Rows.Add(tableWithTime.Rows[tableWithTimeI++].ItemArray);
                        flag = true;
                    }
                    if (tableWithoutTimeI < tableWithoutTime.Rows.Count)
                    {
                        table.Rows.Add(tableWithoutTime.Rows[tableWithoutTimeI++].ItemArray);
                        flag = true;
                    }
                }
                #endregion

                return Request.CreateResponse(HttpStatusCode.OK, table);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Can't get patients list");
            }
        }
       
        /*
         * create new record in visit_status with InProcess status
         */
        public HttpResponseMessage Post(int id)
        {
            try
            {
                bool withTime = true;
                #region Check last patient queue status (have time or not)
                string checkLastVisitTime = @"
                select top 1 visits.visit_time
                from visit_status
                left join statuses ON (visit_status.status_id = statuses.status_id)
                left join visits ON (visits.visit_id = visit_status.visit_id)
                where statuses.status_name = 'Finished'
                order by visit_status.last_update desc";

                DataTable table = new DataTable();
                using (var con = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["test_db"].ConnectionString))
                using (var cmd = new SqlCommand(checkLastVisitTime, con))
                using (var da = new SqlDataAdapter(cmd))
                {
                    var param = cmd.Parameters.Add("@id", SqlDbType.Int);
                    param.Value = id;
                    cmd.CommandType = CommandType.Text;
                    da.Fill(table);
                }
                if (table.Rows.OfType<DataRow>().Any(r => r.ItemArray[0].ToString() == ":бв"))
                    withTime = false;
                #endregion

                int visitId = -1;
                int visitIdWithoutTime = -1;
                int visitIdWithTime = -1;

                //ищем запись, где явно задано время
                #region Find first man without time
                DataTable tableResultWithoutTime = new DataTable();
                string queryWithoutTime = @"
                    select top 1 visits.visit_id
                    from visits
                    left join citizens ON (visits.citizen_id = citizens.citizen_id)
	                left join visit_status ON (visits.visit_id = visit_status.visit_id)
	                left join statuses ON (visit_status.status_id = statuses.status_id)
                    where visits.personal_specialization_code IN (
		                select personal_specializations.personal_specialization_code
		                from personal_specializations
		                where personal_specialization_id = @personalSpecializationId)
	                and (statuses.status_name IS NULL)
                    and convert(date, visits.visit_date) = convert(date, GETDATE())
                    and visits.visit_time = ':бв'
                    order by visits.visit_id";

                using (var con = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["test_db"].ConnectionString))
                using (var cmd = new SqlCommand(queryWithoutTime, con))
                using (var da = new SqlDataAdapter(cmd))
                {
                    var param = cmd.Parameters.Add("@personalSpecializationId", SqlDbType.Int);
                    param.Value = id;
                    cmd.CommandType = CommandType.Text;
                    da.Fill(tableResultWithoutTime);
                }

                if (tableResultWithoutTime.Rows.Count != 0)
                    visitIdWithoutTime = (int)tableResultWithoutTime.Rows[0][0];

                #endregion

                //где время NULL
                #region Find first man with time
                DataTable tableResultWithTime = new DataTable();
                string queryWithTime = @"
                    select top 1 visits.visit_id
                    from visits
                    left join citizens ON (visits.citizen_id = citizens.citizen_id)
	                left join visit_status ON (visits.visit_id = visit_status.visit_id)
	                left join statuses ON (visit_status.status_id = statuses.status_id)
                    where visits.personal_specialization_code IN (
		                select personal_specializations.personal_specialization_code
		                from personal_specializations
		                where personal_specialization_id = @personalSpecializationId)
	                and (statuses.status_name IS NULL)
                    and convert(date, visits.visit_date) = convert(date, GETDATE())
                    and visits.visit_time != ':бв'
                    order by visits.visit_time";

                using (var con = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["test_db"].ConnectionString))
                using (var cmd = new SqlCommand(queryWithTime, con))
                using (var da = new SqlDataAdapter(cmd))
                {
                    var param = cmd.Parameters.Add("@personalSpecializationId", SqlDbType.Int);
                    param.Value = id;
                    cmd.CommandType = CommandType.Text;
                    da.Fill(tableResultWithTime);
                }

                if (tableResultWithTime.Rows.Count != 0)
                    visitIdWithTime = (int)tableResultWithTime.Rows[0][0];
                #endregion

                if (withTime)
                {
                    if (visitIdWithoutTime >= 0)
                        visitId = visitIdWithoutTime;
                    else if (visitIdWithTime >= 0)
                        visitId = visitIdWithTime;
                }
                else
                {
                    if (visitIdWithTime >= 0)
                        visitId = visitIdWithTime;
                    else if (visitIdWithoutTime >= 0)
                        visitId = visitIdWithoutTime;
                }

                if (visitId < 0)
                {
                    custom_exception ex = new custom_exception();
                    ex.error_string = "Очередь пуста";
                    return Request.CreateResponse(HttpStatusCode.OK, ex);
                }

                #region Insert new visit_status
                string querry = @"
                insert into visit_status (visit_id, personal_specialization_id, status_id, last_update)
                values (@visitId, @id, (select top 1 statuses.status_id
                        from statuses
                        where statuses.status_name = 'InProcess')
                    , convert(datetime, GETDATE())
                )";

                DataTable tableAnswer = new DataTable();
                using (var con = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["test_db"].ConnectionString))
                using (var cmd = new SqlCommand(querry, con))
                using (var da = new SqlDataAdapter(cmd))
                {
                    var param = cmd.Parameters.Add("@id", SqlDbType.Int);
                    param.Value = id;
                    var param2 = cmd.Parameters.Add("@visitId", SqlDbType.Int);
                    param2.Value = visitId;
                    cmd.CommandType = CommandType.Text;
                    da.Fill(tableAnswer);
                }
                #endregion

                return Request.CreateResponse(HttpStatusCode.OK, "OK");
            }
            catch (Exception)
            {
                custom_exception ex = new custom_exception();
                ex.error_string = "Ошибка при вызове пациента";
                return Request.CreateResponse(HttpStatusCode.OK, ex);
            }            
        }
        /*
         * Update existing record in visit_status. Set Finished status
         */
        public HttpResponseMessage Put(int id)
        {
            string querry = @"
                update visit_status set status_id = (
                    select top 1 statuses.status_id
                    from statuses
                    where statuses.status_name = 'Finished'
					)
					, last_update = convert(datetime, GETDATE())
                where personal_specialization_id = 1 and status_id = (
                    select top 1 statuses.status_id
                    from statuses
                    where statuses.status_name = 'InProcess')
                ";

            try
            {
                DataTable table = new DataTable();
                using (var con = new SqlConnection(ConfigurationManager.
                    ConnectionStrings["test_db"].ConnectionString))
                using (var cmd = new SqlCommand(querry, con))
                using (var da = new SqlDataAdapter(cmd))
                {
                    var param = cmd.Parameters.Add("@id", SqlDbType.Int);
                    param.Value = id;
                    cmd.CommandType = CommandType.Text;
                    da.Fill(table);
                }

                return Request.CreateResponse(HttpStatusCode.OK, "OK");
            }
            catch (Exception)
            {
                custom_exception ex = new custom_exception();
                ex.error_string = "Ошибка при завершении посещения пациента";
                return Request.CreateResponse(HttpStatusCode.OK, ex);
            }
        }
    }
}