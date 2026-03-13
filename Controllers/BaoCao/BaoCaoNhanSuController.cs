using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Helper.Utils;
using BatDongSan.Models.NhanSu;
using System.Text;
using BatDongSan.Utils.Paging;
using BatDongSan.Helper.Common;
using System.Globalization;
using System.Configuration;
using System.Net.Mail;
using BatDongSan.Models.PhieuDeNghi;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BatDongSan.Controllers.BaoCao
{
    public class BaoCaoNhanSuController : ApplicationController
    {
        private LinqNhanSuDataContext context = new LinqNhanSuDataContext();
        LinqPhieuDeNghiDataContext lqPhieuDN = new LinqPhieuDeNghiDataContext();

        private IList<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan> phongBans;

        private StringBuilder buildTree;
        private readonly string MCV = "BaoCaoNhanSu";
        private bool? permission;
        //
        // GET: /BaoCaoNhanSu/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult BCSinhNhatNV(int? page, int? pageSize, string searchString, int? thang, int? day)
        {
            #region Role user
            permission = GetPermission("BCSinhNhatNV", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            int currentPageIndex = page.HasValue ? page.Value : 1;
            pageSize = pageSize ?? 50;
            IList<sp_BC_NS_SinhNhatNhanVienResult> list;
            BuilThang(DateTime.Now.Month);
            thang = thang.HasValue ? thang : DateTime.Now.Month;
            using (context = new LinqNhanSuDataContext())
            {

                list = context.sp_BC_NS_SinhNhatNhanVien(thang,day, searchString).ToList();
                ViewBag.Count = list.Count();
                ViewBag.searchString = searchString;
                ViewBag.thang = thang;
                TempData["Params"] = searchString + "," + thang;
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("ViewIndexSN", list.ToPagedList(currentPageIndex, 50));
            }
            return View(list.ToPagedList(currentPageIndex, 50));
        }
        private void BuilThang(int thang)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            dict.Add(0, "[Tất cả]");

            for (var i = 1; i <= 12; i++)
            {
                dict.Add(i, i.ToString());
            }
            ViewBag.Thangs = new SelectList(dict, "Key", "Value", thang);
        }
        public ActionResult BCDiTreVeSom(int? page, int? pageSize, string searchString, string tuNgay, string denNgay)
        {
            #region Role user
            permission = GetPermission("BCDiTreVeSom", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            int currentPageIndex = page.HasValue ? page.Value : 1;
            pageSize = pageSize ?? 50;
            IList<sp_BC_NS_NhanVienDiTreVeSomResult> list;
            DateTime? fromDate = null;
            DateTime? toDate = null;
            if (!String.IsNullOrEmpty(tuNgay))
                fromDate = DateTime.ParseExact(tuNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            if (!String.IsNullOrEmpty(denNgay))
                toDate = DateTime.ParseExact(denNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            using (context = new LinqNhanSuDataContext())
            {

                list = context.sp_BC_NS_NhanVienDiTreVeSom(fromDate, toDate, searchString).ToList();
                ViewBag.Count = list.Count();
                ViewBag.searchString = searchString;
                ViewBag.tuNgay = tuNgay;
                ViewBag.denNgay = denNgay;
                TempData["Params"] = searchString + "," + tuNgay + "," + denNgay;
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("ViewIndexDTVS", list.ToPagedList(currentPageIndex, 50));
            }
            return View(list.ToPagedList(currentPageIndex, 50));
        }
        public ActionResult BCNhanVienNghiPhep(int? page, int? pageSize, string searchString, string tuNgay, string denNgay)
        {
            #region Role user
            permission = GetPermission("BCNhanVienNghiPhep", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            int currentPageIndex = page.HasValue ? page.Value : 1;
            pageSize = pageSize ?? 50;
            IList<sp_BC_NS_NhanVienNghiPhepResult> list;
            DateTime? fromDate = null;
            DateTime? toDate = null;
            if (!String.IsNullOrEmpty(tuNgay))
                fromDate = DateTime.ParseExact(tuNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            if (!String.IsNullOrEmpty(denNgay))
                toDate = DateTime.ParseExact(denNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            using (context = new LinqNhanSuDataContext())
            {

                list = context.sp_BC_NS_NhanVienNghiPhep(fromDate, toDate, searchString).ToList();
                ViewBag.Count = list.Count();
                ViewBag.searchString = searchString;
                ViewBag.tuNgay = tuNgay;
                ViewBag.denNgay = denNgay;
                TempData["Params"] = searchString + "," + tuNgay + "," + denNgay;
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("ViewIndexNP", list.ToPagedList(currentPageIndex, 50));
            }
            return View(list.ToPagedList(currentPageIndex, 50));
        }
       
        public ActionResult BCTQNghiPhep()
        {
            #region Role user
            permission = GetPermission("BCTQNghiPhep", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            buildTree = new StringBuilder();
            phongBans = context.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan>().ToList();
            buildTree = TreePhongBanStyle.BuildTreeDepartment(phongBans);
            ViewBag.PhongBans = buildTree.ToString();
            nam(DateTime.Now.Year);
            return View("BCTQNghiPhep");
        }
        public ActionResult BCTQNghiPhepViewIndex(int? pageSize, string searchString, string maPhongBan, int nam, int _page = 0)
        {
            try
            {
                #region Role user
                permission = GetPermission("BCTQNghiPhep", BangPhanQuyen.QuyenXem);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion
                context = new LinqNhanSuDataContext();
                ViewBag.isGet = "True";

                string maNhanVien = GetUser().manv;
                int page = _page == 0 ? 1 : _page;
                int pIndex = page;

                var dataReusults = context.sp_BC_NghiPhep_Index(maPhongBan, searchString, nam).ToList();

                PagingLoaderController("/BaoCaoNhanSu/BCTQNghiPhep/", dataReusults.Count(), page, "?searchString=" + searchString + "&maPhongBan=" + maPhongBan + "&nam=" + nam);
                ViewData["lsDanhSach"] = dataReusults.Skip(start).Take(offset).ToList();

                return PartialView("ViewIndexNPTQ");
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }

        }
        

        public ActionResult BCHopDongSapHetHan(int? page, int? pageSize, string searchString, int? soNgayHetHan)
        {
            #region Role user
            permission = GetPermission("BCHopDongSapHetHan", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            int currentPageIndex = page.HasValue ? page.Value : 1;
            pageSize = pageSize ?? 50;
            IList<sp_BC_NS_HopDongSapHetHanResult> list;

            using (context = new LinqNhanSuDataContext())
            {

                list = context.sp_BC_NS_HopDongSapHetHan(soNgayHetHan, searchString).ToList();
                ViewBag.Count = list.Count();
                ViewBag.searchString = searchString;
                ViewBag.soNgayHetHan = soNgayHetHan;
                TempData["Params"] = searchString + "," + soNgayHetHan;
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("ViewIndexHDHH", list.ToPagedList(currentPageIndex, 50));
            }
            return View(list.ToPagedList(currentPageIndex, 50));
        }
        public ActionResult BCTangCaTrongNam(int? page, int? pageSize, string searchString)
        {
            #region Role user
            permission = GetPermission("BCTangCaTrongNam", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            int currentPageIndex = page.HasValue ? page.Value : 1;
            pageSize = pageSize ?? 50;
            IList<sp_NS_TongSoGioTangCaResult> list;

            using (context = new LinqNhanSuDataContext())
            {

                list = lqPhieuDN.sp_NS_TongSoGioTangCa(searchString).ToList();
                ViewBag.Count = list.Count();
                ViewBag.searchString = searchString;
                TempData["Params"] = searchString;
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("ViewIndexTangCaTrongNam", list.ToPagedList(currentPageIndex, 50));
            }
            return View(list.ToPagedList(currentPageIndex, 50));
        }
        public ActionResult BieuDoLuong()
        {
            thang(DateTime.Now.Month);
            nam(DateTime.Now.Year);
            return View();
        }
        public ActionResult GetListBieuDoLuong(int? thangFrom, int? thangTo, int? namFrom, int? namTo)
        {

            #region Role user
            permission = GetPermission("BieuDoLuong", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {

                var list = context.sp_BC_NS_BieuDoLuongThang(thangFrom, namFrom, thangTo, namTo).ToList();
                var result = new { kq = list };
                return Json(result, JsonRequestBehavior.AllowGet);





            }
            catch
            {
                var result = new { kq = false };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }
        private void thang(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 0; i < 13; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["lstThangFrom"] = new SelectList(dics, "Key", "Value", value);
            ViewData["lstThangTo"] = new SelectList(dics, "Key", "Value", value);

        }
        private void nam(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 2015; i < 2031; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["lstNamFrom"] = new SelectList(dics, "Key", "Value", value);
            ViewData["lstNamTo"] = new SelectList(dics, "Key", "Value", value);
            ViewData["lstNam"] = new SelectList(dics, "Key", "Value", value);
            
        }
        // check mail trong thang da send
        public ActionResult CheckUpdateMailSN(int thang, string type)
        {


            try
            {
                #region Role user
                permission = GetPermission("BCSinhNhatNV", BangPhanQuyen.QuyenXem);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion
                //Check 
                var result = new { kq = false };

              
                if (type == "month")
                {
                     var checkEx = context.tbl_TrangThaiSinhNhats.Where(t => t.nam == DateTime.Now.Year && t.thang == thang && t.day == null).FirstOrDefault();
                     if (checkEx != null)
                     {
                         return Json(result, JsonRequestBehavior.AllowGet);
                     }
                }
                else {
                     var checkEx = context.tbl_TrangThaiSinhNhats.Where(t => t.nam == DateTime.Now.Year && t.thang == thang && t.day == DateTime.Now.Day).FirstOrDefault();
                     if (checkEx != null)
                     {
                         return Json(result, JsonRequestBehavior.AllowGet);
                     }
                }
                    
                //End check
                // Insert Row 
                tbl_TrangThaiSinhNhat tblDuyetBL = new tbl_TrangThaiSinhNhat();
                tblDuyetBL.nam = DateTime.Now.Year;
                tblDuyetBL.thang = thang;
                if (type == "month")
                {
                    tblDuyetBL.day = null;
                }
                else {
                    tblDuyetBL.day = DateTime.Now.Day;
                }
                tblDuyetBL.ngayDuyet = DateTime.Now;
                tblDuyetBL.nguoiDuyet = GetUser().manv;
                context.tbl_TrangThaiSinhNhats.InsertOnSubmit(tblDuyetBL);
                context.SubmitChanges();
                // End Insert Row
                // Check Exist

                
                result = new { kq = true };
                
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                var result = new { kq = false };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }
        // end check mail
        public ActionResult SendMailSNThang(int? thang)
        {
            #region Role user
            permission = GetPermission("BCSinhNhatNV", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            string qSearch = "";
            //không được gửi mail trong năm
            if (thang == null || thang < 1)
            {
                var result1 = new { kq = false };
                return Json(result1, JsonRequestBehavior.AllowGet);
            }

            var listSendMails = context.sp_BC_NS_SinhNhatNhanVien(thang,null, qSearch).ToList();
            foreach (var item in listSendMails)
            {
                // Code send mail
                MailHelper mailInit = new MailHelper(); // lay cac tham so trong webconfig
                System.Text.StringBuilder content = new System.Text.StringBuilder();


                //Content 
                content.Append("<div style=\"width: 493px; margin: 16px auto; line-height: 1.7; font-size: 16px; box-shadow: -3px -3px  #4b6580; padding: 10px; color: green; border: 3px dotted #0c7932; border-radius: 7px;\">");
                content.Append("<p>Xin chào: <span style=\"font-size: 25px; color: #fb4d0b;\">" + item.hoTen + "</span>");
                content.Append("</p>");
                content.Append("<p style=\"font-style: italic; font-size: 30px; color: #3eaf3e; text-align: center;\">Chúc mừng <span style=\"font-size: 30px; text-transform: uppercase; color: #fb4d0b;\">Sinh nhật </span><span style=\"font-size: 37px; color: #d21839; display:block; clear:both; float:none;\">" + thang + "/" + DateTime.Now.Year + "</span>");
                content.Append("</p>");
                content.Append("<p style=\"font-style: italic;\">Thanks and Regards!</p>");
                content.Append("<p style=\"font-style: italic;\">Email từ hệ thống nhân sự</p>");
                content.Append("</div>");
                //End content
                //Send only email is @thuanviet.com.vn
                string[] array01 = item.email.ToLower().Split('@');
                string string2 = ConfigurationManager.AppSettings["OnlySend"]; //get domain from config files
                string[] array1 = string2.Split(',');
                // bool EmailofThuanViet;
                //EmailofThuanViet = array1.Contains(array01[1]);
                // if (emailNV == "" || emailNV == null || EmailofThuanViet == false)
                // {
                //    return false;
                // }
                MailAddress toMail = new MailAddress(item.email, item.hoTen); // goi den mail
                mailInit.ToMail = toMail;
                mailInit.Body = content.ToString();
                mailInit.SendMail();
                // End code send mail
            }
            var result = new { kq = true };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendMailSNNgay(int? thang)
        {
            try
            {
                #region Role user
                permission = GetPermission("BCSinhNhatNV", BangPhanQuyen.QuyenXem);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion

                //không được gửi mail trong năm
                if (thang == null || thang < 1)
                {
                    var result1 = new { kq = false };
                    return Json(result1, JsonRequestBehavior.AllowGet);
                }

                string qSearch = "";

                var listSendMails = context.sp_BC_NS_SinhNhatNhanVien(thang, DateTime.Now.Day, qSearch).ToList();
                foreach (var item in listSendMails)
                {
                    // Code send mail
                    MailHelper mailInit = new MailHelper(); // lay cac tham so trong webconfig
                    System.Text.StringBuilder content = new System.Text.StringBuilder();


                    //Content 
                    content.Append("<div style=\"width: 493px; margin: 16px auto; line-height: 1.7; font-size: 16px; box-shadow: -3px -3px  #4b6580; padding: 10px; color: green; border: 3px dotted #0c7932; border-radius: 7px;\">");
                    content.Append("<p>Xin chào: <span style=\"font-size: 25px; color: #fb4d0b;\">" + item.hoTen + "</span>");
                    content.Append("</p>");
                    content.Append("<p style=\"font-style: italic; font-size: 30px; color: #3eaf3e; text-align: center;\">Chúc mừng <span style=\"font-size: 30px; text-transform: uppercase; color: #fb4d0b;\">Sinh nhật </span><span style=\"font-size: 37px; color: #d21839; display:block; clear:both; float:none;\">" + DateTime.Now.Day + "/" + thang + "</span>");
                    content.Append("</p>");
                    content.Append("<p style=\"font-style: italic;\">Thanks and Regards!</p>");
                    content.Append("<p style=\"font-style: italic;\">Email từ hệ thống nhân sự</p>");
                    content.Append("</div>");
                    //End content
                    //Send only email is @thuanviet.com.vn
                    string[] array01 = item.email.ToLower().Split('@');
                    string string2 = ConfigurationManager.AppSettings["OnlySend"]; //get domain from config files
                    string[] array1 = string2.Split(',');
                    // bool EmailofThuanViet;
                    //EmailofThuanViet = array1.Contains(array01[1]);
                    // if (emailNV == "" || emailNV == null || EmailofThuanViet == false)
                    // {
                    //    return false;
                    // }
                    MailAddress toMail = new MailAddress(item.email, item.hoTen); // goi den mail
                    mailInit.ToMail = toMail;
                    mailInit.Body = content.ToString();
                    mailInit.SendMail();
                    // End code send mail
                }

                var result = new { kq = true, day = DateTime.Now.Day };
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch{
                var result = new { kq = false, day = DateTime.Now.Day };
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }


        public void XuatFileBaoCaoSinhNhatNhanVien(string searchString, int? thang)
        {

            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return;
            if (!permission.Value)
                return;
            #endregion

            var filename = "";
            var virtualPath = HttpRuntime.AppDomainAppVirtualPath;

            filename += "DanhSachSinhNhat_" + DateTime.Today.Year + "_" + thang + ".xlsx";


            

            using (ExcelPackage package = new ExcelPackage())
            {
                //Create a sheet
                package.Workbook.Worksheets.Add("DanhSachSinhNhat_" + DateTime.Today.Year + "_" + thang);

                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                //Header
                //insert từ dòng nào, bao nhiêu row
                var rowFrom = 1;
                worksheet.InsertRow(rowFrom, 1);
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Mã nhân viên";
                worksheet.Cells[1, 3].Value = "Tên nhân viên";
                worksheet.Cells[1, 4].Value = "Phòng ban/Chức vụ";
                worksheet.Cells[1, 5].Value = "Ngày sinh";
                worksheet.Cells[1, 6].Value = "Số tuổi";
                worksheet.Cells[1, 7].Value = "Thâm niên";
                worksheet.Cells[1, 8].Value = "Ngày vào làm";

                worksheet.Column(1).Width = 5;
                worksheet.Column(2).Width = 10;
                worksheet.Column(3).Width = 25;
                worksheet.Column(4).Width = 40;
                worksheet.Column(5).Width = 15;
                worksheet.Column(6).Width = 10;
                worksheet.Column(7).Width = 10;
                worksheet.Column(8).Width = 15;


                #region
                //Body
                var list = context.sp_BC_NS_SinhNhatNhanVien(thang, null, searchString).ToList();

                if (list != null && list.Count > 0)
                {
                    var countSTT = 1;
                    foreach (var item in list)
                    {

                        rowFrom = rowFrom + 1;
                        worksheet.InsertRow(rowFrom, 1);
                        worksheet.Cells[rowFrom, 1].Value = countSTT++;
                        worksheet.Cells[rowFrom, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                        

                        worksheet.Cells[rowFrom, 2].Value = item.maNhanVien;
                        worksheet.Cells[rowFrom, 2].Style.Font.Bold = true;

                        worksheet.Cells[rowFrom, 3].Value = item.hoTen;
                        worksheet.Cells[rowFrom, 4].Value = item.tenPhongBan;


                        worksheet.Cells[rowFrom, 5].Value = string.Format("{0:dd/MM/yyyy}", item.ngaySinh);
                        worksheet.Cells[rowFrom, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;

                        worksheet.Cells[rowFrom, 6].Value = (item.soTuoi??0).ToString();
                        worksheet.Cells[rowFrom, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;

                        worksheet.Cells[rowFrom, 7].Value = (item.thamNien??0).ToString();
                        worksheet.Cells[rowFrom, 7].Style.Numberformat.Format = "#,##0.000";
                        worksheet.Cells[rowFrom, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;

                        worksheet.Cells[rowFrom, 8].Value = string.Format("{0:dd/MM/yyyy}", item.ngayVaoLam);
                        worksheet.Cells[rowFrom, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;

                    }
                }

                #endregion

                //Generate A File
                Byte[] bin = package.GetAsByteArray();

                Response.BinaryWrite(bin);
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", string.Format("attachment;filename={0}", filename));
            }
        }

    }
}
