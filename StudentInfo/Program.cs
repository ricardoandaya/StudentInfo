using StudentInfo.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StudentInfo
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("https://studentinfoapi-81rs.onrender.com/api/students");
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                IStudentDataServices studentDataServices = new StudentAPIService(httpClient);
                Application.Run(new Form(studentDataServices));
            }
        }
    }
}
