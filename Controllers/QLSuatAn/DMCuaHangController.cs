using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Helper.Common;
using BatDongSan.Models.DanhMuc;
using BatDongSan.Models.QLSuatAn;
using BatDongSan.Helper.Utils;
using System.Globalization;
using System.IO;
using BatDongSan.Models.QLSuatAn.LuuVet;
using System.Threading;
using System.Net;
using System.Net.Mail;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel.Contrib;
using Worldsoft.Mvc.Web.Util;
using System.Text;
using BatDongSan.Models.NhanSu;

namespace BatDongSan.Controllers.QLSuatAn
{
    public class DMCuaHangController : ApplicationController
    {
        private QLSALuuVetDataContext context = new QLSALuuVetDataContext();
        private IList<tbl_SA_CuaHang> cuaHangs;
        private tbl_SA_CuaHang cuaHang;
        private readonly string MCV = "DMCuaHang";
        private bool? permission;
        private string mimeType;
        private string duongDan;
        private LinqNhanSuDataContext nhanSuContext = new LinqNhanSuDataContext();
        private QLSuatAnDataContext qLSAContext = new QLSuatAnDataContext();
        private StringBuilder buildTree;
        private IList<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan> phongBans;

        public ActionResult Index()
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            using (context = new QLSALuuVetDataContext())
            {
                cuaHangs = context.tbl_SA_CuaHangs.ToList();
            }

            //Phân bổ - start
            buildTree = new StringBuilder();
            phongBans = nhanSuContext.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan>().ToList();
            buildTree = TreePhongBanStyle.BuildTreeDepartment(phongBans);

            ViewBag.PhongBans = buildTree.ToString();

            thang(DateTime.Now.Month);
            nam(DateTime.Now.Year);
            thuocCongTy(string.Empty);
            //Phân bổ - end


            return View(cuaHangs);
        }


        private void thuocCongTy(string value)
        {
            Dictionary<string, string> dics = new Dictionary<string, string>();

            dics[""] = "[Tất cả]";
            dics["ThuanViet"] = "Thuận Việt";
            dics["TVWindow"] = "TV Window";
            dics["NewCity"] = "NewCity";
            dics["Cienco6"] = "Cienco6";

            ViewData["thuocCongTys"] = new SelectList(dics, "Key", "Value", value);
        }

        private void thang(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 0; i < 13; i++)
            {
                dics[i] = i.ToString();
            }

            ViewData["thangs"] = new SelectList(dics, "Key", "Value", value);
        }


        private void nam(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 2015; i < 2031; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["nams"] = new SelectList(dics, "Key", "Value", value);
        }

        //
        // GET: /BangCap/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /BangCap/Create

        public ActionResult Create()
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenThem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            cuaHang = new tbl_SA_CuaHang();
            cuaHang.capQuanLy = false;
            cuaHang.maCuaHang = GenerateUtil.CheckLetter("CH", GetMax());


            return PartialView("Create", cuaHang);
        }


        public string GetMax()
        {
            context = new QLSALuuVetDataContext();

            return context.tbl_SA_CuaHangs.OrderByDescending(d => d.ngayLap).Select(d => d.maCuaHang).FirstOrDefault() ?? string.Empty;
        }

        public string GetMaxMonAn()
        {
            context = new QLSALuuVetDataContext();

            return context.tbl_SA_CuaHang_MonAns.OrderByDescending(d => d.maMonAn).Select(d => d.maMonAn).FirstOrDefault() ?? string.Empty;
        }


        //
        // POST: /BangCap/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection, HttpPostedFileBase[] files)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenThem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            try
            {
                cuaHang = new tbl_SA_CuaHang();

                using (context = new QLSALuuVetDataContext())
                {
                    duongDan = String.Empty;
                    cuaHang.maCuaHang = GenerateUtil.CheckLetter("CH", GetMax());
                    cuaHang.tenCuaHang = collection["tenCuaHang"];
                    cuaHang.ghiChu = collection["ghiChu"];
                    cuaHang.soSuatAnTrongNgay = Convert.ToInt32(string.IsNullOrEmpty(collection["soSuatAnTrongNgay"]) ? "0" : collection["soSuatAnTrongNgay"]);
                    cuaHang.suatAnTrenNhanVien = Convert.ToInt32(string.IsNullOrEmpty(collection["suatAnTrenNhanVien"]) ? "0" : collection["suatAnTrenNhanVien"]);
                    cuaHang.ngayLap = DateTime.Now;
                    cuaHang.nguoiLap = GetUser().manv;
                    FileUploading(files[0]);
                    cuaHang.avatar = duongDan;
                    try
                    {
                        cuaHang.capQuanLy = collection["capQuanLy"].Contains("true") ? true : false;
                    }
                    catch
                    {
                        cuaHang.capQuanLy = false;
                    }

                    try
                    {
                        cuaHang.lamTronLenTyLePhanBo = collection["lamTronLenTyLePhanBo"].Contains("true") ? true : false;
                    }
                    catch
                    {
                        cuaHang.lamTronLenTyLePhanBo = false;
                    }

                    context.tbl_SA_CuaHangs.InsertOnSubmit(cuaHang);

                    context.SubmitChanges();
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        //Save Image vào database
        private bool FileUploading(HttpPostedFileBase file)
        {
            duongDan = null;
            if (file != null && file.ContentLength > 0)
            {
                mimeType = file.ContentType;
                var date = DateTime.Now.ToString("yyyyMMdd-HHMMss") + ".jpg";
                var filePathOriginal = Server.MapPath("/Images/CuaHang/");
                string savedFileName = Path.Combine(filePathOriginal, date.ToString());
                duongDan = "/Images/CuaHang/" + date.ToString();
                file.SaveAs(savedFileName);
                return true;
            }
            else
                return false;
        }
        private string GenerateRandowm()
        {
            string strPwdchar = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string strPwd = "";
            Random rnd = new Random();
            for (int i = 0; i <= 15; i++)
            {
                int iRandom = rnd.Next(0, strPwdchar.Length - 1);
                strPwd += strPwdchar.Substring(iRandom, 1);
            }
            return strPwd;
        }
        private bool FileUploadingMonAn(HttpPostedFileBase fileMonAn)
        {
            duongDan = null;
            if (fileMonAn != null && fileMonAn.ContentLength > 0)
            {
                mimeType = fileMonAn.ContentType;

                var date = GenerateRandowm() + DateTime.Now.ToString("yyyyMMdd-HHMMss") + ".jpg";
                var filePathOriginal = Server.MapPath("/Images/MonAn/");
                string savedFileName = Path.Combine(filePathOriginal, date.ToString());
                duongDan = "/Images/MonAn/" + date.ToString();
                fileMonAn.SaveAs(savedFileName);
                return true;
            }
            else
                return false;
        }

        //
        // GET: /BangCap/Edit/5

        public ActionResult Edit(int id)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            using (context = new QLSALuuVetDataContext())
            {
                cuaHang = context.tbl_SA_CuaHangs.Where(s => s.id == id).FirstOrDefault();
            }
            return PartialView("Edit", cuaHang);
        }
        public ActionResult DanhSachMonAn(string id)
        {
            context = new QLSALuuVetDataContext();
            var lstCuaHangMonAn = context.tbl_SA_CuaHang_MonAns.Where(d => d.maCuaHang == id).ToList();
            ViewBag.tblCuaHang = context.tbl_SA_CuaHangs.Where(d => d.maCuaHang == id).FirstOrDefault();
            return PartialView("DanhSachMonAn", lstCuaHangMonAn);
        }


        public ActionResult DanhSachNhanVien(string qsearch, string maCuaHang)
        {
            context = new QLSALuuVetDataContext();

            var lstData = context.sp_CuaHang_MonAn_NhanVien_Index(qsearch, maCuaHang, System.Configuration.ConfigurationManager.AppSettings["CongTy"].ToString()).ToList();

            return PartialView("DanhSachNhanVien", lstData);
        }


        //
        // POST: /BangCap/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection, HttpPostedFileBase[] files)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            try
            {
                using (context = new QLSALuuVetDataContext())
                {
                    cuaHang = context.tbl_SA_CuaHangs.Where(s => s.id == id).FirstOrDefault();

                    cuaHang.tenCuaHang = collection["tenCuaHang"];
                    cuaHang.ghiChu = collection["ghiChu"];
                    cuaHang.soSuatAnTrongNgay = Convert.ToInt32(string.IsNullOrEmpty(collection["soSuatAnTrongNgay"]) ? "0" : collection["soSuatAnTrongNgay"]);
                    cuaHang.suatAnTrenNhanVien = Convert.ToInt32(string.IsNullOrEmpty(collection["suatAnTrenNhanVien"]) ? "0" : collection["suatAnTrenNhanVien"]);
                    try
                    {
                        cuaHang.capQuanLy = collection["capQuanLy"].Contains("true") ? true : false;
                    }
                    catch
                    {
                        cuaHang.capQuanLy = false;
                    }

                    try
                    {
                        cuaHang.lamTronLenTyLePhanBo = collection["lamTronLenTyLePhanBo"].Contains("true") ? true : false;
                    }
                    catch
                    {
                        cuaHang.lamTronLenTyLePhanBo = false;
                    }

                    duongDan = null; mimeType = null;

                    if (files[0] != null)
                    {
                        FileUploading(files[0]);
                        cuaHang.avatar = duongDan;

                    }
                    context.SubmitChanges();
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // POST: /BangCap/Delete/5
        [HttpPost]
        public ActionResult DanhSachMonAn(string id, FormCollection collection, HttpPostedFileBase[] fileMonAn)
        {
            context = new QLSALuuVetDataContext();


            string[] monAns = collection.GetValues("tenMonAn");


            if (monAns != null)
            {
                for (int i = 0; i < monAns.Count(); i++)
                {

                    if (!string.IsNullOrEmpty(collection.GetValues("tenMonAn")[i]))
                    {
                        var maMonAn = collection.GetValues("maMonAn")[i];
                        if (!String.IsNullOrEmpty(maMonAn))
                        {
                            var tblSAMonAn = context.tbl_SA_CuaHang_MonAns.Where(d => d.maMonAn == maMonAn).FirstOrDefault();
                            if (tblSAMonAn != null)
                            {
                                Thread.Sleep(1000);
                                tblSAMonAn.tenMonAn = collection.GetValues("tenMonAn")[i];
                                tblSAMonAn.soLuongMonAn = Convert.ToInt32(string.IsNullOrEmpty(collection.GetValues("soLuongMonAn")[i]) ? "0" : collection.GetValues("soLuongMonAn")[i]);
                                tblSAMonAn.maCuaHang = id;
                                tblSAMonAn.ngayLap = DateTime.Now;
                                tblSAMonAn.nguoiLap = GetUser().manv;
                                duongDan = null; mimeType = null;
                                if (fileMonAn[i] != null)
                                {
                                    FileUploadingMonAn(fileMonAn[i]);
                                    tblSAMonAn.avatar = duongDan;
                                }
                                context.SubmitChanges();
                            }
                        }
                        else
                        {
                            Thread.Sleep(2000);
                            tbl_SA_CuaHang_MonAn tblCuaHangMonAn = new tbl_SA_CuaHang_MonAn();
                            tblCuaHangMonAn.tenMonAn = collection.GetValues("tenMonAn")[i];
                            tblCuaHangMonAn.maMonAn = GenerateUtil.CheckLetter("MA", GetMaxMonAn());
                            tblCuaHangMonAn.soLuongMonAn = Convert.ToInt32(string.IsNullOrEmpty(collection.GetValues("soLuongMonAn")[i]) ? "0" : collection.GetValues("soLuongMonAn")[i]);
                            tblCuaHangMonAn.maCuaHang = id;
                            tblCuaHangMonAn.ngayLap = DateTime.Now;
                            tblCuaHangMonAn.nguoiLap = GetUser().manv;
                            duongDan = null; mimeType = null;
                            if (fileMonAn[i] != null)
                            {
                                FileUploadingMonAn(fileMonAn[i]);
                                tblCuaHangMonAn.avatar = duongDan;
                            }
                            context.tbl_SA_CuaHang_MonAns.InsertOnSubmit(tblCuaHangMonAn);
                            context.SubmitChanges();

                        }



                    }
                }
            }
            return RedirectToAction("Index");

        }
        public JsonResult DeleteMonAn(string maMonAn)
        {

            var checkLuuVet = context.tbl_SA_PhieuDangKySuatAn_LuuVets.Where(d => d.maMonAn == maMonAn).FirstOrDefault();
            if (checkLuuVet != null)
            {
                return Json("false");
            }
            else
            {
                var tblMonAn = context.tbl_SA_CuaHang_MonAns.Where(d => d.maMonAn == maMonAn).FirstOrDefault();
                context.tbl_SA_CuaHang_MonAns.DeleteOnSubmit(tblMonAn);
                context.SubmitChanges();
            }
            return Json("true");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXoa);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            try
            {
                using (context = new QLSALuuVetDataContext())
                {
                    cuaHang = context.tbl_SA_CuaHangs.Where(s => s.id == id).FirstOrDefault();

                    context.tbl_SA_CuaHangs.DeleteOnSubmit(cuaHang);

                    context.SubmitChanges();
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        private string TaoFileExcelMonAnChoNhanVien(string maCuaHang)
        {

            //tạo file xls
            var virtualPath = HttpRuntime.AppDomainAppVirtualPath;
            var date = DateTime.Now.ToString("yyyyMMdd-HHMMss");
            string savedLocation = "/UploadFiles/QLSuatAn/";
            var filePath = Server.MapPath(savedLocation);

            if (System.IO.File.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            var sourceFile = System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\UploadFiles\Template\TemplateDatMonAn.xls");
            var destFile = System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\UploadFiles\QLSuatAn\" + maCuaHang + "_" + date + ".xls");

            System.IO.File.Copy(sourceFile, destFile);

            var fileStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);

            var workbook = new HSSFWorkbook(fileStream, true);

            var sheet = workbook.GetSheet("danhsachnhanvien");

            #region
            /*style title start*/
            //tạo font cho các title
            //font tiêu đề 
            HSSFFont hFontTieuDe = (HSSFFont)workbook.CreateFont();
            hFontTieuDe.FontHeightInPoints = 18;
            hFontTieuDe.Boldweight = 100 * 10;
            hFontTieuDe.FontName = "Times New Roman";
            hFontTieuDe.Color = HSSFColor.BLUE.index;
            HSSFFont hFontTieuDe2 = (HSSFFont)workbook.CreateFont();
            hFontTieuDe2.FontHeightInPoints = 15;
            hFontTieuDe2.Boldweight = 100 * 10;
            hFontTieuDe2.FontName = "Times New Roman";
            hFontTieuDe2.Color = HSSFColor.BLACK.index;

            //font tiêu đề 
            HSSFFont hFontTongGiaTriHT = (HSSFFont)workbook.CreateFont();
            hFontTongGiaTriHT.FontHeightInPoints = 11;
            hFontTongGiaTriHT.Boldweight = (short)FontBoldWeight.BOLD;
            hFontTongGiaTriHT.FontName = "Times New Roman";
            hFontTongGiaTriHT.Color = HSSFColor.BLACK.index;

            //font thông tin bảng tính
            HSSFFont hFontTT = (HSSFFont)workbook.CreateFont();
            hFontTT.IsItalic = true;
            hFontTT.Boldweight = (short)FontBoldWeight.BOLD;
            hFontTT.Color = HSSFColor.BLACK.index;
            hFontTT.FontName = "Times New Roman";
            hFontTieuDe.FontHeightInPoints = 11;

            //font chứ hoa đậm
            HSSFFont hFontNommalUpper = (HSSFFont)workbook.CreateFont();
            hFontNommalUpper.Boldweight = (short)FontBoldWeight.BOLD;
            hFontNommalUpper.Color = HSSFColor.BLACK.index;
            hFontNommalUpper.FontName = "Times New Roman";

            //font chữ bình thường
            HSSFFont hFontNommal = (HSSFFont)workbook.CreateFont();
            hFontNommal.Color = HSSFColor.BLACK.index;
            hFontNommal.FontName = "Times New Roman";

            //font chữ bình thường đậm
            HSSFFont hFontNommalBold = (HSSFFont)workbook.CreateFont();
            hFontNommalBold.Color = HSSFColor.BLACK.index;
            hFontNommalBold.Boldweight = (short)FontBoldWeight.BOLD;
            hFontNommalBold.FontName = "Times New Roman";

            //tạo font cho các title end

            //Set style
            var styleTitle = workbook.CreateCellStyle();
            styleTitle.SetFont(hFontTieuDe);
            styleTitle.Alignment = HorizontalAlignment.LEFT;
            var styleTitle1 = workbook.CreateCellStyle();
            styleTitle1.SetFont(hFontTieuDe2);
            styleTitle1.Alignment = HorizontalAlignment.CENTER;

            //style infomation
            var styleInfomation = workbook.CreateCellStyle();
            styleInfomation.SetFont(hFontTT);
            styleInfomation.Alignment = HorizontalAlignment.LEFT;

            //style header
            var styleheadedColumnTable = workbook.CreateCellStyle();
            styleheadedColumnTable.SetFont(hFontNommalUpper);
            styleheadedColumnTable.WrapText = true;
            styleheadedColumnTable.BorderBottom = CellBorderType.THIN;
            styleheadedColumnTable.BorderLeft = CellBorderType.THIN;
            styleheadedColumnTable.BorderRight = CellBorderType.THIN;
            styleheadedColumnTable.BorderTop = CellBorderType.THIN;
            styleheadedColumnTable.VerticalAlignment = VerticalAlignment.CENTER;
            styleheadedColumnTable.Alignment = HorizontalAlignment.CENTER;

            var styleHeading1 = workbook.CreateCellStyle();
            styleHeading1.SetFont(hFontNommalBold);
            styleHeading1.WrapText = true;
            styleHeading1.BorderBottom = CellBorderType.THIN;
            styleHeading1.BorderLeft = CellBorderType.THIN;
            styleHeading1.BorderRight = CellBorderType.THIN;
            styleHeading1.BorderTop = CellBorderType.THIN;
            styleHeading1.VerticalAlignment = VerticalAlignment.CENTER;
            styleHeading1.Alignment = HorizontalAlignment.LEFT;

            var hStyleConLeft = (HSSFCellStyle)workbook.CreateCellStyle();
            hStyleConLeft.SetFont(hFontNommal);
            hStyleConLeft.VerticalAlignment = VerticalAlignment.TOP;
            hStyleConLeft.Alignment = HorizontalAlignment.LEFT;
            hStyleConLeft.WrapText = true;
            hStyleConLeft.BorderBottom = CellBorderType.THIN;
            hStyleConLeft.BorderLeft = CellBorderType.THIN;
            hStyleConLeft.BorderRight = CellBorderType.THIN;
            hStyleConLeft.BorderTop = CellBorderType.THIN;

            var hStyleConRight = (HSSFCellStyle)workbook.CreateCellStyle();
            hStyleConRight.SetFont(hFontNommal);
            hStyleConRight.VerticalAlignment = VerticalAlignment.TOP;
            hStyleConRight.Alignment = HorizontalAlignment.RIGHT;
            hStyleConRight.BorderBottom = CellBorderType.THIN;
            hStyleConRight.BorderLeft = CellBorderType.THIN;
            hStyleConRight.BorderRight = CellBorderType.THIN;
            hStyleConRight.BorderTop = CellBorderType.THIN;


            var hStyleConCenter = (HSSFCellStyle)workbook.CreateCellStyle();
            hStyleConCenter.SetFont(hFontNommal);
            hStyleConCenter.VerticalAlignment = VerticalAlignment.TOP;
            hStyleConCenter.Alignment = HorizontalAlignment.CENTER;
            hStyleConCenter.BorderBottom = CellBorderType.THIN;
            hStyleConCenter.BorderLeft = CellBorderType.THIN;
            hStyleConCenter.BorderRight = CellBorderType.THIN;
            hStyleConCenter.BorderTop = CellBorderType.THIN;
            //set style end
            #endregion

            Row rowC = null;
            //Khai báo row đầu tiên
            int firstRowNumber = 1;

            string rowtitle = "Danh sách nhân viên đặt món tại cửa hàng :" + context.tbl_SA_CuaHangs.Where(d => d.maCuaHang == maCuaHang).Select(d => d.tenCuaHang).FirstOrDefault() ?? string.Empty;
            var titleCell = HSSFCellUtil.CreateCell(sheet.CreateRow(firstRowNumber), 2, rowtitle.ToUpper());
            titleCell.CellStyle = styleTitle;
            firstRowNumber = firstRowNumber + 2;
            string rowtitle1 = "Ngày : " + string.Format("{0:dd/MM/yyyy}", DateTime.Now);
            var titleCell1 = HSSFCellUtil.CreateCell(sheet.CreateRow(firstRowNumber), 2, rowtitle1);
            titleCell1.CellStyle = styleTitle1;

            firstRowNumber = firstRowNumber + 2;

            var list1 = new List<string>();
            list1.Add("STT");
            list1.Add("Tên nhân viên");
            list1.Add("Tên phòng ban");
            list1.Add("Tên món ăn");

            list1.Add("Công ty thuộc");
            //list1.Add("Số điện thoại");

            //Start row 13
            var headerRow = sheet.CreateRow(6);
            ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);

            //Create header end

            var idRowStart = 6;
            var datas = context.sp_CuaHang_MonAn_NhanVien_Index(string.Empty, maCuaHang, System.Configuration.ConfigurationManager.AppSettings["CongTy"].ToString()).ToList();
            //#region
            if (datas != null && datas.Count > 0)
            {
                var stt = 0;
                int dem = 0;
                //Giai đoạn
                foreach (var item1 in datas)
                {
                    dem = 0;
                    stt++;
                    idRowStart++;
                    rowC = sheet.CreateRow(idRowStart);
                    ReportHelperExcel.SetAlignment(rowC, dem++, stt.ToString(), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.tenNhanVien, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.tenPhongBan, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.tenMonAn, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, System.Configuration.ConfigurationManager.AppSettings["CongTy"].ToString(), hStyleConLeft);
                }

                sheet.SetColumnWidth(0, 8 * 210);
                sheet.SetColumnWidth(1, 30 * 210);
                sheet.SetColumnWidth(2, 30 * 210);
                sheet.SetColumnWidth(3, 30 * 210);
            }
            else
            {
                sheet.SetColumnWidth(0, 8 * 210);
                sheet.SetColumnWidth(1, 30 * 210);
                sheet.SetColumnWidth(2, 30 * 210);
                sheet.SetColumnWidth(3, 30 * 210);
            }

            var stream = new MemoryStream();
            workbook.Write(stream);

            using (FileStream file = new FileStream(destFile, FileMode.Create, System.IO.FileAccess.Write))
            {
                byte[] bytes = new byte[stream.GetBuffer().Length];
                stream.Read(bytes, 0, (int)stream.GetBuffer().Length);
                file.Write(bytes, 0, bytes.Length);
                stream.Close();
            }

            return destFile;
        }


        public string SendMailCuaHang(string maCuaHang)
        {
            try
            {
                var cuaHang = context.tbl_SA_CuaHangs.Where(d => d.maCuaHang == maCuaHang).FirstOrDefault();

                if (cuaHang != null)
                {
                    if (!string.IsNullOrEmpty(cuaHang.email))
                    {
                        string[] BCCs = null;
                        var client = new WebClient();
                        int i = 0;
                        int maThongBao = 0;
                        string noiDung = String.Empty;
                        string tieuDe = String.Empty;

                        tbl_SA_CuaHang_TinhTrangGuiMail tinhTrangGuiMail = new tbl_SA_CuaHang_TinhTrangGuiMail();

                        List<string> attachments = new List<string>();

                        string desFile = TaoFileExcelMonAnChoNhanVien(maCuaHang);

                        //var fullDownload = "https://nhansu.thuanviet.com.vn/UploadFiles/MThongBao/" + file.savedFileName;

                        //client.DownloadFile(fullDownload, AppDomain.CurrentDomain.BaseDirectory + file.savedFileName);
                        attachments.Add(desFile);

                        //Thread.Sleep(2000);
                        MailHelper mailInit = new MailHelper();
                        MailAddress toMail = new MailAddress(cuaHang.email, string.Empty);

                        mailInit.ToMail = toMail;
                        mailInit.Subject = tieuDe;
                        //mailInit.listToEmail = listEmail;
                        mailInit.Body = noiDung;

                        mailInit.AttachmentPaths = attachments.ToArray();

                        if (mailInit.SendMail())
                        {
                            tinhTrangGuiMail.ghiChu = "gửi mail thành công";
                        }
                        else
                        {
                            tinhTrangGuiMail.ghiChu = "gửi mail thất bại";
                        }

                        tinhTrangGuiMail.maCuaHang = maCuaHang;
                        tinhTrangGuiMail.ngayLap = DateTime.Now;
                        tinhTrangGuiMail.nguoiLap = GetUser().manv;

                        context.SubmitChanges();

                        return string.Empty;
                    }
                    else
                    {
                        return "Vui lòng set địa chỉ email cho cửa hàng.";
                    }
                }
                else
                {
                    return "Vui lòng chọn cửa hàng.";
                }



            }
            catch
            {
                return "Có lỗi xảy ra, vui lòng liên hệ bộ phận IT.";
            }
        }

        public void XuatFileMonAnNhanVien(string maCuaHang, string thuocCongTy)
        {
            var filename = "";
            var virtualPath = HttpRuntime.AppDomainAppVirtualPath;
            var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplate.xls"), FileMode.Open, FileAccess.Read);

            var workbook = new HSSFWorkbook(fileStream, true);
            filename += "maCuaHang" + string.Format("{0:dd/MM/yyyy}", DateTime.Now) + ".xls";


            var sheet = workbook.GetSheet("danhsachnhanvien");

            /*style title start*/
            //tạo font cho các title
            //font tiêu đề 
            HSSFFont hFontTieuDe = (HSSFFont)workbook.CreateFont();
            hFontTieuDe.FontHeightInPoints = 18;
            hFontTieuDe.Boldweight = 100 * 10;
            hFontTieuDe.FontName = "Times New Roman";
            hFontTieuDe.Color = HSSFColor.BLUE.index;
            HSSFFont hFontTieuDe2 = (HSSFFont)workbook.CreateFont();
            hFontTieuDe2.FontHeightInPoints = 15;
            hFontTieuDe2.Boldweight = 100 * 10;
            hFontTieuDe2.FontName = "Times New Roman";
            hFontTieuDe2.Color = HSSFColor.BLACK.index;

            //font tiêu đề 
            HSSFFont hFontTongGiaTriHT = (HSSFFont)workbook.CreateFont();
            hFontTongGiaTriHT.FontHeightInPoints = 11;
            hFontTongGiaTriHT.Boldweight = (short)FontBoldWeight.BOLD;
            hFontTongGiaTriHT.FontName = "Times New Roman";
            hFontTongGiaTriHT.Color = HSSFColor.BLACK.index;

            //font thông tin bảng tính
            HSSFFont hFontTT = (HSSFFont)workbook.CreateFont();
            hFontTT.IsItalic = true;
            hFontTT.Boldweight = (short)FontBoldWeight.BOLD;
            hFontTT.Color = HSSFColor.BLACK.index;
            hFontTT.FontName = "Times New Roman";
            hFontTieuDe.FontHeightInPoints = 11;

            //font chứ hoa đậm
            HSSFFont hFontNommalUpper = (HSSFFont)workbook.CreateFont();
            hFontNommalUpper.Boldweight = (short)FontBoldWeight.BOLD;
            hFontNommalUpper.Color = HSSFColor.BLACK.index;
            hFontNommalUpper.FontName = "Times New Roman";

            //font chữ bình thường
            HSSFFont hFontNommal = (HSSFFont)workbook.CreateFont();
            hFontNommal.Color = HSSFColor.BLACK.index;
            hFontNommal.FontName = "Times New Roman";

            //font chữ bình thường đậm
            HSSFFont hFontNommalBold = (HSSFFont)workbook.CreateFont();
            hFontNommalBold.Color = HSSFColor.BLACK.index;
            hFontNommalBold.Boldweight = (short)FontBoldWeight.BOLD;
            hFontNommalBold.FontName = "Times New Roman";

            //tạo font cho các title end

            //Set style
            var styleTitle = workbook.CreateCellStyle();
            styleTitle.SetFont(hFontTieuDe);
            styleTitle.Alignment = HorizontalAlignment.LEFT;
            var styleTitle1 = workbook.CreateCellStyle();
            styleTitle1.SetFont(hFontTieuDe2);
            styleTitle1.Alignment = HorizontalAlignment.CENTER;

            //style infomation
            var styleInfomation = workbook.CreateCellStyle();
            styleInfomation.SetFont(hFontTT);
            styleInfomation.Alignment = HorizontalAlignment.LEFT;

            //style header
            var styleheadedColumnTable = workbook.CreateCellStyle();
            styleheadedColumnTable.SetFont(hFontNommalUpper);
            styleheadedColumnTable.WrapText = true;
            styleheadedColumnTable.BorderBottom = CellBorderType.THIN;
            styleheadedColumnTable.BorderLeft = CellBorderType.THIN;
            styleheadedColumnTable.BorderRight = CellBorderType.THIN;
            styleheadedColumnTable.BorderTop = CellBorderType.THIN;
            styleheadedColumnTable.VerticalAlignment = VerticalAlignment.CENTER;
            styleheadedColumnTable.Alignment = HorizontalAlignment.CENTER;

            var styleHeading1 = workbook.CreateCellStyle();
            styleHeading1.SetFont(hFontNommalBold);
            styleHeading1.WrapText = true;
            styleHeading1.BorderBottom = CellBorderType.THIN;
            styleHeading1.BorderLeft = CellBorderType.THIN;
            styleHeading1.BorderRight = CellBorderType.THIN;
            styleHeading1.BorderTop = CellBorderType.THIN;
            styleHeading1.VerticalAlignment = VerticalAlignment.CENTER;
            styleHeading1.Alignment = HorizontalAlignment.LEFT;

            var hStyleConLeft = (HSSFCellStyle)workbook.CreateCellStyle();
            hStyleConLeft.SetFont(hFontNommal);
            hStyleConLeft.VerticalAlignment = VerticalAlignment.TOP;
            hStyleConLeft.Alignment = HorizontalAlignment.LEFT;
            hStyleConLeft.WrapText = true;
            hStyleConLeft.BorderBottom = CellBorderType.THIN;
            hStyleConLeft.BorderLeft = CellBorderType.THIN;
            hStyleConLeft.BorderRight = CellBorderType.THIN;
            hStyleConLeft.BorderTop = CellBorderType.THIN;

            var hStyleConRight = (HSSFCellStyle)workbook.CreateCellStyle();
            hStyleConRight.SetFont(hFontNommal);
            hStyleConRight.VerticalAlignment = VerticalAlignment.TOP;
            hStyleConRight.Alignment = HorizontalAlignment.RIGHT;
            hStyleConRight.BorderBottom = CellBorderType.THIN;
            hStyleConRight.BorderLeft = CellBorderType.THIN;
            hStyleConRight.BorderRight = CellBorderType.THIN;
            hStyleConRight.BorderTop = CellBorderType.THIN;


            var hStyleConCenter = (HSSFCellStyle)workbook.CreateCellStyle();
            hStyleConCenter.SetFont(hFontNommal);
            hStyleConCenter.VerticalAlignment = VerticalAlignment.TOP;
            hStyleConCenter.Alignment = HorizontalAlignment.CENTER;
            hStyleConCenter.BorderBottom = CellBorderType.THIN;
            hStyleConCenter.BorderLeft = CellBorderType.THIN;
            hStyleConCenter.BorderRight = CellBorderType.THIN;
            hStyleConCenter.BorderTop = CellBorderType.THIN;
            //set style end


            Row rowC = null;
            //Khai báo row đầu tiên
            int firstRowNumber = 1;

            string rowtitle = "Danh sách nhân viên đặt món tại cửa hàng :" + context.tbl_SA_CuaHangs.Where(d => d.maCuaHang == maCuaHang).Select(d => d.tenCuaHang).FirstOrDefault() ?? "Tất cả cửa hàng";
            var titleCell = HSSFCellUtil.CreateCell(sheet.CreateRow(firstRowNumber), 2, rowtitle.ToUpper());
            titleCell.CellStyle = styleTitle;
            firstRowNumber = firstRowNumber + 2;
            string rowtitle1 = "Ngày : " + string.Format("{0:dd/MM/yyyy}", DateTime.Now);
            var titleCell1 = HSSFCellUtil.CreateCell(sheet.CreateRow(firstRowNumber), 2, rowtitle1);
            titleCell1.CellStyle = styleTitle1;

            firstRowNumber = firstRowNumber + 2;

            var list1 = new List<string>();
            list1.Add("STT");
            list1.Add("Tên nhân viên");
            list1.Add("Tên phòng ban");
            //list1.Add("Tên món ăn");
            list1.Add("Thời gian phục vụ");

            list1.Add("Công ty thuộc");
            //list1.Add("Số điện thoại");

            //Start row 13
            var headerRow = sheet.CreateRow(6);
            ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);

            //Create header end

            var idRowStart = 6;
            var datas = context.sp_CuaHang_MonAn_NhanVien_Index(string.Empty, maCuaHang, thuocCongTy).ToList();
            //#region
            if (datas != null && datas.Count > 0)
            {
                var stt = 0;
                int dem = 0;
                //Giai đoạn
                foreach (var item1 in datas)
                {
                    dem = 0;
                    stt++;
                    idRowStart++;
                    rowC = sheet.CreateRow(idRowStart);
                    ReportHelperExcel.SetAlignment(rowC, dem++, stt.ToString(), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.tenNhanVien, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.tenPhongBan, hStyleConLeft);
                    //ReportHelperExcel.SetAlignment(rowC, dem++, item1.tenMonAn, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.thoiGianPhucVu, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.tenCongTy, hStyleConLeft);
                }

                sheet.SetColumnWidth(0, 8 * 210);
                sheet.SetColumnWidth(1, 30 * 210);
                sheet.SetColumnWidth(2, 30 * 210);
                sheet.SetColumnWidth(3, 30 * 210);
            }
            else
            {
                sheet.SetColumnWidth(0, 8 * 210);
                sheet.SetColumnWidth(1, 30 * 210);
                sheet.SetColumnWidth(2, 30 * 210);
                sheet.SetColumnWidth(3, 30 * 210);
            }

            var stream = new MemoryStream();
            workbook.Write(stream);

            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", filename));
            Response.Clear();

            Response.BinaryWrite(stream.GetBuffer());
            Response.End();

        }


        //private List<tbl_SA_NhanVienDangKySuatAnTrongThang> GetNhanVienDangKyTatCaCongTy(int thang, int nam)
        //{
        //    List<tbl_SA_NhanVienDangKySuatAnTrongThang> nhanViens = new List<tbl_SA_NhanVienDangKySuatAnTrongThang>();
        //    tbl_SA_NhanVienDangKySuatAnTrongThang nhanVien = null;

        //    //phân bổ suất ăn của Thuận Việt
        //    var suatAnNhanViens = qLSAContext.sp_SA_DangKySuatAn_CongChuanNhanVien(thang, nam).ToList();

        //    if (suatAnNhanViens != null && suatAnNhanViens.Count() > 0)
        //    {
        //        foreach (var item in suatAnNhanViens)
        //        {
        //            nhanVien = new tbl_SA_NhanVienDangKySuatAnTrongThang();

        //            nhanVien.congChuan = item.congChuan;
        //            nhanVien.createDay = DateTime.Now;
        //            nhanVien.maNhanVien = item.maNhanVien;
        //            nhanVien.nam = nam;
        //            nhanVien.thang = thang;
        //            nhanVien.tenNhanVien = item.tenNhanVien;
        //            nhanVien.tenPhongBan = item.tenPhongBan;
        //            nhanVien.thuocCongTy = "ThuanViet";

        //            nhanViens.Add(nhanVien);
        //        }
        //    }

        //    //phân bổ suất ăn của NewCity
        //    HRNewCityDataContext lQNewCity = new HRNewCityDataContext();

        //    var suatAnNhanVienNewcitys = lQNewCity.sp_SA_DangKySuatAn_CongChuanNhanVien_NewCiTy(thang, nam).ToList();

        //    if (suatAnNhanVienNewcitys != null && suatAnNhanVienNewcitys.Count() > 0)
        //    {
        //        foreach (var item in suatAnNhanVienNewcitys)
        //        {
        //            nhanVien = new tbl_SA_NhanVienDangKySuatAnTrongThang();

        //            nhanVien.congChuan = item.congChuan;
        //            nhanVien.createDay = DateTime.Now;
        //            nhanVien.maNhanVien = item.maNhanVien;
        //            nhanVien.nam = nam;
        //            nhanVien.thang = thang;
        //            nhanVien.tenNhanVien = item.tenNhanVien;
        //            nhanVien.tenPhongBan = item.tenPhongBan;
        //            nhanVien.thuocCongTy = "NewCity";

        //            nhanViens.Add(nhanVien);
        //        }
        //    }

        //    //phân bổ suất ăn của TVWindow
        //    LQNhanSuTVWindowDataContext lQTVWindow = new LQNhanSuTVWindowDataContext();

        //    var suatAnNhanVienTVWindows = lQTVWindow.sp_SA_DangKySuatAn_CongChuanNhanVien_TVWindow(thang, nam).ToList();

        //    if (suatAnNhanVienTVWindows != null && suatAnNhanVienTVWindows.Count() > 0)
        //    {
        //        foreach (var item in suatAnNhanVienTVWindows)
        //        {
        //            nhanVien = new tbl_SA_NhanVienDangKySuatAnTrongThang();

        //            nhanVien.congChuan = item.congChuan;
        //            nhanVien.createDay = DateTime.Now;
        //            nhanVien.maNhanVien = item.maNhanVien;
        //            nhanVien.nam = nam;
        //            nhanVien.thang = thang;
        //            nhanVien.tenNhanVien = item.tenNhanVien;
        //            nhanVien.tenPhongBan = item.tenPhongBan;
        //            nhanVien.thuocCongTy = "TVWindow";

        //            nhanViens.Add(nhanVien);
        //        }
        //    }


        //    //phân bổ suất ăn của Cienco6
        //    LQNhanSuCienco6DataContext lQCienco6 = new LQNhanSuCienco6DataContext();

        //    var suatAnNhanVienCienco6s = lQCienco6.sp_SA_DangKySuatAn_CongChuanNhanVien_Cienco6(thang, nam).ToList();

        //    if (suatAnNhanVienCienco6s != null && suatAnNhanVienCienco6s.Count() > 0)
        //    {
        //        foreach (var item in suatAnNhanVienCienco6s)
        //        {
        //            nhanVien = new tbl_SA_NhanVienDangKySuatAnTrongThang();

        //            nhanVien.congChuan = item.congChuan;
        //            nhanVien.createDay = DateTime.Now;
        //            nhanVien.maNhanVien = item.maNhanVien;
        //            nhanVien.nam = nam;
        //            nhanVien.thang = thang;
        //            nhanVien.tenNhanVien = item.tenNhanVien;
        //            nhanVien.tenPhongBan = item.tenPhongBan;
        //            nhanVien.thuocCongTy = "Cienco6";

        //            nhanViens.Add(nhanVien);
        //        }
        //    }

        //    return nhanViens;
        //}

        //public ActionResult PhanBoSuatAn(int thang, int nam)
        //{

        //    #region Role user
        //    permission = GetPermission(MCV, BangPhanQuyen.QuyenXoa);
        //    if (!permission.HasValue)
        //        return Json(new { kq = false, message = "LogIn" });
        //    if (!permission.Value)
        //        return Json(new { kq = false, message = "AccessDenied" });
        //    #endregion

        //    try
        //    {
        //        //kiểm tra đã phân bổ hay chưa - start
        //        if (context.tbl_SA_DuyetPhanBos.Where(d => d.thang == thang && d.nam == nam).Count() > 0)
        //        {
        //            return Json(new { kq = false, message = "Tháng này đã duyệt phân bổ." });
        //        }
        //        //kiểm tra đã phân bổ hay chưa - end


        //        List<tbl_SA_NhanVienDangKySuatAnTrongThang> nhanViens = GetNhanVienDangKyTatCaCongTy(thang, nam);

        //        //tiến hành xóa -- start
        //        var deleteNhanVienThuanViets = context.tbl_SA_NhanVienDangKySuatAnTrongThangs.Where(d => d.nam == nam && d.thang == thang
        //            && d.thuocCongTy == "ThuanViet");

        //        var deleteNhanVienNewCitys = context.tbl_SA_NhanVienDangKySuatAnTrongThangs.Where(d => d.nam == nam && d.thang == thang
        //            && d.thuocCongTy == "NewCity");

        //        var deleteNhanVienTVWindows = context.tbl_SA_NhanVienDangKySuatAnTrongThangs.Where(d => d.nam == nam && d.thang == thang
        //            && d.thuocCongTy == "TVWindow");

        //        var deleteNhanVienCienco6s = context.tbl_SA_NhanVienDangKySuatAnTrongThangs.Where(d => d.nam == nam && d.thang == thang
        //            && d.thuocCongTy == "Cienco6");

        //        context.tbl_SA_NhanVienDangKySuatAnTrongThangs.DeleteAllOnSubmit(deleteNhanVienThuanViets);
        //        context.tbl_SA_NhanVienDangKySuatAnTrongThangs.DeleteAllOnSubmit(deleteNhanVienNewCitys);
        //        context.tbl_SA_NhanVienDangKySuatAnTrongThangs.DeleteAllOnSubmit(deleteNhanVienTVWindows);
        //        context.tbl_SA_NhanVienDangKySuatAnTrongThangs.DeleteAllOnSubmit(deleteNhanVienCienco6s);
        //        //tiến hành xóa -- end

        //        context.tbl_SA_NhanVienDangKySuatAnTrongThangs.InsertAllOnSubmit(nhanViens);

        //        context.SubmitChanges();

        //        //Sau khi tiến hành đưa data xuống db suất ăn, thì tính toán lại phần phân bổ - Start
        //        context.sp_SA_NhanVienDangKySuatAnTrongThang_PhanBo(thang, nam);
        //        //Sau khi tiến hành đưa data xuống db suất ăn, thì tính toán lại phần phân bổ - End

        //        return Json(new { kq = true });
        //    }
        //    catch
        //    {
        //        return Json(new { kq = false, message = "Có lỗi trong quá trình xử lý, liên hệ bộ phận IT." });
        //    }
        //}


        public ActionResult LoadDanhSachPhanBo(int thang, int nam)
        {

            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion


            var data = context.sp_SA_PhanBoCuaHangCongChuan_Index(thang, nam).ToList();


            ViewData["lsDanhSach"] = data;

            //kiểm tra button
            //kiểm tra đã phân bổ hay chưa - start
            if (context.tbl_SA_DuyetPhanBos.Where(d => d.thang == thang && d.nam == nam).Count() > 0)
            {
                ViewData["isDuyetPhanBo"] = true;
            }
            else
            {
                ViewData["isDuyetPhanBo"] = false;
            }

            return PartialView("_LoadDanhSachPhanBo");
        }

        public ActionResult DuyetPhanBoSuatAn(int thang, int nam)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return Json(new { kq = false, message = "LogIn" });
            if (!permission.Value)
                return Json(new { kq = false, message = "AccessDenied" });
            #endregion

            try
            {
                tbl_SA_DuyetPhanBo duyetPhanBo = new tbl_SA_DuyetPhanBo();

                duyetPhanBo.createDay = DateTime.Now;
                duyetPhanBo.createUser = GetUser().manv;
                duyetPhanBo.nam = nam;
                duyetPhanBo.thang = thang;

                context.tbl_SA_DuyetPhanBos.InsertOnSubmit(duyetPhanBo);

                context.SubmitChanges();

                return Json(new { kq = true });
            }
            catch
            {
                return Json(new { kq = false, message = "Có lỗi trong quá trình xử lý, liên hệ bộ phận IT." });
            }
        }

        public ActionResult LoadDanhSachBaoCaoPhanBo(int thang, int nam)
        {

            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            QLSALuuVetDataContext linqSALuuVet = new QLSALuuVetDataContext();
            var data = linqSALuuVet.sp_SA_BC_ThongKeSuatAnTheoTungCuaHang(thang, nam).ToList();

            //ViewData["nhanVienTatCas"] = GetNhanVienDangKyTatCaCongTy(thang, nam);

            ViewData["lsDanhSach"] = data;

            return PartialView("_LoadDanhSachBaoCaoPhanBo");
        }

    }
}
