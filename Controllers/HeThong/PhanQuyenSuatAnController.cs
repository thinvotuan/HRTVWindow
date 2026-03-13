using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Models.HeThong;
using BatDongSan.Helper.Common;
using BatDongSan.Utils.Paging;
using System.Text;
using BatDongSan.Helper.Utils;
using BatDongSan.Models.DanhMuc;
using BatDongSan.Models.QLSuatAn;
using BatDongSan.Models.QLSuatAn.LuuVet;
using System.Transactions;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel.Contrib;
using Worldsoft.Mvc.Web.Util;

namespace BatDongSan.Controllers.HeThong
{
    public class PhanQuyenSuatAnController : ApplicationController
    {
        private LinqHeThongDataContext context = new LinqHeThongDataContext();
        private IList<Sys_NhomUser> userGroups;
        private IList<sp_Sys_CongViecCuaUsersResult> quyenCuaNhomUsers;
        private IList<sp_Sys_User_IndexResult> userOfGroups;
        private IList<sp_Sys_VuViecCuaCongViecResult> vuViecCuaCongViecs;
        private Sys_NhomUser nhomUser;
        private Sys_CongViecVaVuViec congViecVaVuViec;
        private bool? permission;
        //private static int defaultPageSize = 20;
        private readonly string MCV = "PhanQuyenSuatAn";
        public StringBuilder buildTree;
        public ActionResult Index()
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            userGroups = context.Sys_NhomUsers.Where(d => d.maNhomUser == "DangKySA" || d.maNhomUser == "InPhieuSA" || d.maNhomUser == "CapQuanLySA").ToList();
            if (Request.IsAjaxRequest())
            {
                return PartialView("PartialContent", userGroups);
            }
            return View(userGroups);
        }

        //
        // GET: /PhanQuyen/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /PhanQuyen/Create


        //
        // POST: /PhanQuyen/Create



        //
        // GET: /PhanQuyen/Edit/5

        public ActionResult Edit(string id)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            buildTree = new StringBuilder();
            List<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan> phongBans = context.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan>().ToList();
            buildTree = TreePhongBanStyle.BuildTreeDepartment(phongBans);

            ViewBag.PhongBans = buildTree.ToString();
            ViewBag.NhanVienThuocNhoms = context.sp_Sys_User_Index(id, null, null, null, null).ToList();
            nhomUser = context.Sys_NhomUsers.Where(s => s.maNhomUser == id).FirstOrDefault();
            return View(nhomUser);
        }

        //
        // POST: /PhanQuyen/Edit/5

        [HttpPost]
        public ActionResult Edit(string id, FormCollection collection)
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
                var list = context.Sys_UserThuocNhoms.Where(s => s.maNhomUser == id);
                context.Sys_UserThuocNhoms.DeleteAllOnSubmit(list);

                nhomUser = context.Sys_NhomUsers.Where(s => s.maNhomUser == id).FirstOrDefault();
                nhomUser.tenNhomUser = collection["tenNhomUser"];
                nhomUser.ghiChu = collection["ghiChu"];
                //Cập nhật lại danh sách user thuộc nhóm
                IList<Sys_UserThuocNhom> userThuocNhoms = new List<Sys_UserThuocNhom>();
                string[] userIds = collection.GetValues("userId");
                if (userIds != null)
                {
                    for (int i = 0; i < userIds.Length; i++)
                    {
                        try
                        {
                            if (userIds[i] != "" && userIds[i] != ",")
                            {
                                Sys_UserThuocNhom userThuocNhom = new Sys_UserThuocNhom();
                                userThuocNhom.maNhomUser = nhomUser.maNhomUser;
                                userThuocNhom.userId = Convert.ToInt32(userIds[i]);
                                userThuocNhoms.Add(userThuocNhom);
                            }
                        }
                        catch { 
                        
                        }
                    }
                    context.Sys_UserThuocNhoms.InsertAllOnSubmit(userThuocNhoms);
                }

                var nhomUsers = context.Sys_NhomUsers.Where(s => s.maNhomUser != nhomUser.maNhomUser && s.tenNhomUser == nhomUser.tenNhomUser).ToList();
                if (nhomUsers.Count() <= 0)
                {
                    context.SubmitChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    if (nhomUsers.Where(s => s.maNhomUser.ToLower() == nhomUser.maNhomUser.ToLower()).Count() > 0) TempData["MessgId"] = "Mã nhóm user đã tồn tại";
                    if (nhomUsers.Where(s => s.tenNhomUser.ToLower() == nhomUser.tenNhomUser.ToLower()).Count() > 0) TempData["MessgTen"] = "Tên nhóm user đã tồn tại";
                    return View(nhomUser);
                }
            }
            catch
            {
                return View();
            }
        }
        public JsonResult DeleteUserPQ(int userID, string maNhomUser)
        {
            try
            {
                var tbluserThuocNhom = context.GetTable<Sys_UserThuocNhom>().Where(d => d.maNhomUser == maNhomUser && d.userId == userID).FirstOrDefault();
                
                if (tbluserThuocNhom != null)
                {
                    context.Sys_UserThuocNhoms.DeleteOnSubmit(tbluserThuocNhom);
                    context.SubmitChanges();    
                }

                //phân bổ suất ăn của Thuận Việt - start
                QLSALuuVetDataContext linQ_LuuVet = new QLSALuuVetDataContext();

                string maNhanVien = context.Sys_Users.Where(d => d.userId == userID).Select(d => d.manv).FirstOrDefault() ?? string.Empty;

                var nhanVien = linQ_LuuVet.tbl_SA_NhanVienDangKySuatAnTrongThangs.Where(d => d.maNhanVien == maNhanVien
                    && d.thang == DateTime.Now.Month && d.nam == DateTime.Now.Year
                    && d.thuocCongTy == System.Configuration.ConfigurationManager.AppSettings["CongTy"].ToString()).FirstOrDefault();

                if (nhanVien != null)
                {
                    linQ_LuuVet.tbl_SA_NhanVienDangKySuatAnTrongThangs.DeleteOnSubmit(nhanVien);
                    linQ_LuuVet.SubmitChanges();
                }
                //phân bổ suất ăn của Thuận Việt - end

                //thực hiện transaction
                //using (var trans = new TransactionScope())
                //{
                //    if (tbluserThuocNhom != null)
                //    {
                //        context.SubmitChanges();    
                //    }

                //    if (nhanVien != null)
                //    {
                //        linQ_LuuVet.SubmitChanges();
                //    }

                //    trans.Complete();

                //    return Json("true");
                //}

                return Json("true");
            }
            catch
            {
                return Json("false");
            }
        }
        //
        // POST: /PhanQuyen/Delete/5

        [HttpPost]
        public ActionResult Delete(string[] nhomUser)
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
                var nhomUsers = context.Sys_NhomUsers.Where(s => nhomUser.Contains(s.maNhomUser));
                context.Sys_NhomUsers.DeleteAllOnSubmit(nhomUsers);
                context.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult GetNhanVien(string id)
        {
            ViewBag.Get = true;
            userOfGroups = context.sp_Sys_User_Index(id, null, null, null, null).ToList();
            return PartialView("PartialNhanVien", userOfGroups);
        }

        public ActionResult QuyenTruyCap(string id, bool? viewall)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogOn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                ViewBag.NhomUser = context.Sys_NhomUsers.Where(s => s.maNhomUser == id).FirstOrDefault();
                if (viewall == true)
                {
                    var groups = context.Sys_NhomUsers;
                    ViewBag.NhomUsers = new SelectList(groups, "maNhomUser", "tenNhomUser", id);
                    id = null;
                }
                quyenCuaNhomUsers = context.sp_Sys_CongViecCuaUsers(id, null).ToList();
                return View(quyenCuaNhomUsers);
            }
            catch (Exception e)
            {
                return View("Error", e.Message);
            }
        }

        public ActionResult XemChiTietQuyenHan(string congViec, string nhomUser)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            try
            {
                ViewBag.XemChiTiet = true;
                vuViecCuaCongViecs = context.sp_Sys_VuViecCuaCongViec(congViec, nhomUser).ToList();
                return PartialView("PartialChiTietQuyenHan", vuViecCuaCongViecs);
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult UpdateVuViec(string maVuViec, string maCongViec, string maNhomUser)
        {
            try
            {
                var maCha = context.Sys_CongViecs.Where(s => s.maCongViec == maCongViec)
                                                    .Select(s => s.maCha)
                                                    .FirstOrDefault();
                if (maCha != null && context.Sys_CongViecVaVuViecs.Where(s => s.maCongViec == maCha && s.maNhomUser == maNhomUser).FirstOrDefault() == null)
                {
                    congViecVaVuViec = new Sys_CongViecVaVuViec();
                    congViecVaVuViec.maVuViec = "001";
                    congViecVaVuViec.maNhomUser = maNhomUser;
                    congViecVaVuViec.maCongViec = maCha;
                    context.Sys_CongViecVaVuViecs.InsertOnSubmit(congViecVaVuViec);
                }

                congViecVaVuViec = context.Sys_CongViecVaVuViecs
                                           .Where(s => s.maVuViec == maVuViec
                                               && s.maCongViec == maCongViec
                                               && s.maNhomUser == maNhomUser)
                                           .FirstOrDefault();
                if (congViecVaVuViec != null)
                {
                    context.Sys_CongViecVaVuViecs.DeleteOnSubmit(congViecVaVuViec);
                }
                else
                {
                    congViecVaVuViec = new Sys_CongViecVaVuViec();
                    congViecVaVuViec.maVuViec = maVuViec;
                    congViecVaVuViec.maCongViec = maCongViec;
                    congViecVaVuViec.maNhomUser = maNhomUser;
                    context.Sys_CongViecVaVuViecs.InsertOnSubmit(congViecVaVuViec);
                }
                context.SubmitChanges();
                return Json(new { success = true });
            }
            catch
            {
                return View();
            }
        }

        public ActionResult GetMoreUsers(int? page, string nhomUser, string searchString, string maPhongBan)
        {
            int currentPageIndex = page.HasValue ? page.Value : 1;
            int? tongSoDong = 0;
            var users = context.sp_Sys_User_Index(null, maPhongBan, searchString, currentPageIndex, 20).ToList();
            try
            {
                ViewBag.Count = users[0].tongSoDong;
                tongSoDong = users[0].tongSoDong;
            }
            catch
            {
                ViewBag.Count = 0;
            }
            TempData["Params"] = nhomUser + "," + searchString + "," + maPhongBan;
            return PartialView("PartialUsers", users.ToPagedList(currentPageIndex, 20, true, tongSoDong));
        }
        public ActionResult ChonNhanVienSuatAn()
        {
            LinqDanhMucDataContext linqDanhMuc = new LinqDanhMucDataContext();
            buildTree = new StringBuilder();
            List<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan> phongBans = linqDanhMuc.tbl_DM_PhongBans.ToList();
            buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
            ViewBag.NVPB = buildTree.ToString();
            return PartialView("_NhanVienSuatAn");
        }
        public ActionResult LoadNhanVienSuatAn(int? page, string searchString, string maPhongBan)
        {
            LinqDanhMucDataContext linqDanhMuc = new LinqDanhMucDataContext();
            IList<sp_PB_DanhSachNhanVienResult> phongBan1s;
            phongBan1s = linqDanhMuc.sp_PB_DanhSachNhanVien(searchString, maPhongBan).ToList();
            ViewBag.isGet = "True";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            ViewBag.Count = phongBan1s.Count();
            ViewBag.Search = searchString;
            ViewBag.MaPhongBan = maPhongBan;
            return PartialView("LoadNhanVienSuatAn", phongBan1s.ToPagedList(currentPageIndex, 10));
        }
        public ActionResult LoadPartialNhanVienSuatAnSelected(string maNhomUser)
        {
            ViewBag.NhanVienThuocNhoms = context.sp_Sys_User_Index(maNhomUser, null, null, null, null).ToList();
            return PartialView("LoadPartialNhanVienSuatAnSelected");
        }

        public string updateListNVSuatAn(string MaNV, string maNhomUser)
        {

            try
            {
                // Update
                string[] MaNVs = MaNV.Split(',');
                List<Sys_UserThuocNhom> userThuocNhoms = new List<Sys_UserThuocNhom>();
                List<String> maNhanViens = new List<string>();
                for (int i = 0; i < MaNVs.Length; i++)
                {

                    try
                    {
                        if (context.Sys_UserThuocNhoms.Where(d => d.userId == Convert.ToInt32(MaNVs[i]) && d.maNhomUser == maNhomUser).FirstOrDefault() == null)
                        {
                            Sys_UserThuocNhom userThuocNhom = new Sys_UserThuocNhom();
                            userThuocNhom.maNhomUser = maNhomUser;
                            userThuocNhom.userId = Convert.ToInt32(MaNVs[i]);
                            userThuocNhoms.Add(userThuocNhom);
                        }
                        var tblNhanVien = context.GetTable<Sys_User>().Where(d => d.userId == Convert.ToInt32(MaNVs[i])).FirstOrDefault();
                        if (tblNhanVien != null)
                        {
                            maNhanViens.Add(tblNhanVien.manv);
                        }

                    }
                    catch
                    {

                    }

                }
                context.Sys_UserThuocNhoms.InsertAllOnSubmit(userThuocNhoms);
                context.SubmitChanges();

                //phân bổ suất ăn của Thuận Việt - start
                QLSuatAnDataContext qLSAContext = new QLSuatAnDataContext();
                var suatAnNhanViens = qLSAContext.sp_SA_DangKySuatAn_CongChuanNhanVien_TVWindow(DateTime.Now.Month, DateTime.Now.Year).Where(d => maNhanViens.Contains(d.maNhanVien)).ToList();
                QLSALuuVetDataContext linQ_LuuVet = new QLSALuuVetDataContext();

                if (suatAnNhanViens != null && suatAnNhanViens.Count() > 0)
                {
                    List<tbl_SA_NhanVienDangKySuatAnTrongThang> nhanViens = new List<tbl_SA_NhanVienDangKySuatAnTrongThang>();
                    
                    foreach (var item in suatAnNhanViens)
                    {var checkNhanVien = linQ_LuuVet.tbl_SA_NhanVienDangKySuatAnTrongThangs.Where(d => d.maNhanVien == item.maNhanVien && d.thang == DateTime.Now.Month
                            && d.nam == DateTime.Now.Year).FirstOrDefault();
                    if (checkNhanVien == null)
                    {
                        var nhanVien = new tbl_SA_NhanVienDangKySuatAnTrongThang();

                        nhanVien.congChuan = item.congChuan;
                        nhanVien.createDay = DateTime.Now;
                        nhanVien.maNhanVien = item.maNhanVien;
                        nhanVien.nam = DateTime.Now.Year;
                        nhanVien.thang = DateTime.Now.Month;
                        nhanVien.tenNhanVien = item.tenNhanVien;
                        nhanVien.tenPhongBan = item.tenPhongBan;
                        nhanVien.thuocCongTy = System.Configuration.ConfigurationManager.AppSettings["CongTy"].ToString();

                        nhanViens.Add(nhanVien);
                    }
                    }

                    linQ_LuuVet.tbl_SA_NhanVienDangKySuatAnTrongThangs.InsertAllOnSubmit(nhanViens);
                    linQ_LuuVet.SubmitChanges();    
                }

                return "true";
            }
            catch (Exception ex)
            {
                return "Lỗi: " + ex.ToString();
            }
        }


        public void XuatFileDanhSachNhanVien(string maNhomUser, string maPhongBan)
        {
            var filename = "";
            var virtualPath = HttpRuntime.AppDomainAppVirtualPath;
            var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplate.xls"), FileMode.Open, FileAccess.Read);

            var workbook = new HSSFWorkbook(fileStream, true);
            filename += "DanhSachNhanVien" + string.Format("{0:dd/MM/yyyy}", DateTime.Now) + ".xls";


            var sheet = workbook.GetSheet("danhsachnhanvien");

            /*style title start*/
            //tạo font cho các title
            //font tiêu đề 
            #region

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

            string rowtitle = "Danh sách nhân viên được phân quyền lập phiễu ăn.";
            var titleCell = HSSFCellUtil.CreateCell(sheet.CreateRow(firstRowNumber), 1, rowtitle.ToUpper());
            titleCell.CellStyle = styleTitle;
            firstRowNumber = firstRowNumber + 2;
            string rowtitle1 = "Ngày xuất file: " + string.Format("{0:dd/MM/yyyy}", DateTime.Now);
            var titleCell1 = HSSFCellUtil.CreateCell(sheet.CreateRow(firstRowNumber), 1, rowtitle1);
            titleCell1.CellStyle = styleTitle1;

            firstRowNumber = firstRowNumber + 2;

            var list1 = new List<string>();
            list1.Add("STT");
            list1.Add("Mã nhân viên");
            list1.Add("Họ tên");
            list1.Add("Phòng ban");
            //Start row 13
            var headerRow = sheet.CreateRow(6);
            ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);

            //Create header end

            var idRowStart = 6;
            var datas = context.sp_Sys_User_Index(maNhomUser, maPhongBan, null, null, null).ToList();
            //#region
            if (datas != null && datas.Count > 0)
            {
                var stt = 0;
                int dem = 0;
                //Giai đoạn
                foreach (var item1 in datas.OrderBy(d => d.tenPhongBan))
                {
                    dem = 0;

                    stt++;
                    idRowStart++;
                    rowC = sheet.CreateRow(idRowStart);
                    ReportHelperExcel.SetAlignment(rowC, dem++, stt.ToString(), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.manv, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.HovaTen, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.tenPhongBan, hStyleConLeft);
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
    }
}
