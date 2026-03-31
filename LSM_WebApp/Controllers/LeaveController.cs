using LSM_WebApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LSM_WebApp.Controllers
{
    public class LeaveController : Controller
    {
        string connStr = ConfigurationManager.ConnectionStrings["LMSConnection"].ConnectionString;

        public ActionResult MyRequests()
        {
            int currentUserId = 1; 
            List<LeaveRequest> list = new List<LeaveRequest>();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM LeaveRequests WHERE UserID = @uid", con);
                cmd.Parameters.AddWithValue("@uid", currentUserId);
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new LeaveRequest
                    {
                        RequestID = (int)rdr["RequestID"],
                        LeaveType = rdr["LeaveType"].ToString(),
                        StartDate = (DateTime)rdr["StartDate"],
                        EndDate = (DateTime)rdr["EndDate"],
                        Status = rdr["Status"].ToString(),
                        Reason = rdr["Reason"].ToString()

                    });
                }
            }
            return View(list);
        }

        [HttpPost]
        public ActionResult Apply(LeaveRequest lr)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    SqlCommand cmd = new SqlCommand("sp_ApplyLeave", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", 1);
                    cmd.Parameters.AddWithValue("@LeaveType", lr.LeaveType);
                    cmd.Parameters.AddWithValue("@StartDate", lr.StartDate);
                    cmd.Parameters.AddWithValue("@EndDate", lr.EndDate);
                    cmd.Parameters.AddWithValue("@Reason", lr.Reason);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("MyRequests");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("MyRequests");
            }
        }

        public ActionResult ManagerDashboard(string status, string type)
        {
            List<LeaveRequest> list = new List<LeaveRequest>();
            string query = "SELECT * FROM LeaveRequests WHERE 1=1";
            if (!string.IsNullOrEmpty(status)) query += " AND Status = @status";
            if (!string.IsNullOrEmpty(type)) query += " AND LeaveType = @type";

            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@status", status);
                if (!string.IsNullOrEmpty(type)) cmd.Parameters.AddWithValue("@type", type);
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new LeaveRequest
                    {
                        RequestID = (int)rdr["RequestID"],
                        UserID = (int)rdr["UserID"],
                        LeaveType = rdr["LeaveType"].ToString(),
                        StartDate = (DateTime)rdr["StartDate"],
                        EndDate = (DateTime)rdr["EndDate"],
                        Status = rdr["Status"].ToString(),
                        ManagerComment = rdr["ManagerComment"] != DBNull.Value ? rdr["ManagerComment"].ToString() : ""
                    });
                }
            }
            return View(list);
        }

        [HttpPost]
        public ActionResult ProcessLeave(int reqId, string status, string comment)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("sp_ApproveRejectLeave", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@RequestID", reqId);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@Comment", comment);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("ManagerDashboard");
        }
    }
}