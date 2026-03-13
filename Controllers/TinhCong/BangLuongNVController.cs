using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Helper.Common;
using BatDongSan.Utils.Paging;
using BatDongSan.Helper.Utils;
using System.Data;
using System.Globalization;
using System.Text;
using System.IO;
using BatDongSan.Models.NhanSu;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using System.Configuration;
using System.Net.Mail;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel.Contrib;
using Worldsoft.Mvc.Web.Util;
using NPOI.SS.Util;
using NPOI.HSSF.Util;
using BatDongSan.Models.DanhMuc;
using BatDongSan.Models.ERP;
namespace BatDongSan.Controllers.TinhCong
{
    public class BangLuongNVController : ApplicationController
    {

        private LinqNhanSuDataContext nhanSuContext = new LinqNhanSuDataContext();
        ERPTVWINDOWDataContext lqThuanViet = new ERPTVWINDOWDataContext();
        private readonly string MCV = "BangLuongNV";
        private IList<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan> phongBans;
        private StringBuilder buildTree;
        private bool? permission;
        public ActionResult Index()
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            buildTree = new StringBuilder();
            phongBans = nhanSuContext.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan>().ToList();
            buildTree = TreePhongBanAjax.BuildTreeDepartment(phongBans);
            ViewBag.PhongBans = buildTree.ToString();
            thang(DateTime.Now.Month);
            nam(DateTime.Now.Year);
            ViewBag.ListTaoDNCL = nhanSuContext.GetTable<tbl_NS_KhoiTaoDeNghiChiLuong>().Where(d => d.trangThaiTaoDNCL == true).ToList();
            return View("");

        }

        public ActionResult LoadBangLuongNV(string maPhongBan, string qSearch, int thang, int nam, int _page = 0)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            int page = _page == 0 ? 1 : _page;
            int pIndex = page;
            // int total = nhanSuContext.sp_NS_BangLuongNhanVien(maPhongBan, thang, nam, qSearch).Count();
            //  PagingLoaderController("/BangLuongNV/LoadBangLuongNV/", total, page, "?qsearch=" + qSearch);
            //ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangLuongNhanVien(maPhongBan, thang, nam, qSearch).Skip(start).Take(offset).ToList();
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangLuongNhanVien(maPhongBan, thang, nam, qSearch).ToList();
            ViewData["qSearch"] = qSearch;
            var kqCheck = 0;
            var checkEx = nhanSuContext.tbl_DuyetBangLuongNVs.Where(t => t.nam == nam && t.thang == thang).FirstOrDefault();
            if (checkEx != null)
            {
                kqCheck = 1;
            }
            ViewData["kqCheck"] = kqCheck;
            return PartialView("_LoadBangLuongNV");
        }
        public ActionResult LoadBangLuongNVTinhLai(string maPhongBan, string qSearch, int thang, int nam, int _page = 0)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            int page = _page == 0 ? 1 : _page;
            int pIndex = page;
            int total = nhanSuContext.sp_NS_BangLuongNhanVien_TinhLaiLuong_Index(thang, nam, qSearch).Count();
            PagingLoaderController("/BangLuongNV/LoadBangLuongNVTinhLai/", total, page, "?qsearch=" + qSearch);
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangLuongNhanVien_TinhLaiLuong_Index(thang, nam, qSearch).Skip(start).Take(offset).ToList();

            ViewData["qSearch"] = qSearch;

            return PartialView("_LoadBangLuongNVTinhLai");
        }
        public ActionResult LoadBangLuongNVDieuChinh(string maPhongBan, string qSearch, int thang, int nam, int _page = 0)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            int page = _page == 0 ? 1 : _page;
            int pIndex = page;
            int total = nhanSuContext.sp_NS_BangLuongNhanVien_DieuChinhLuong(maPhongBan, thang, nam, qSearch).Count();
            PagingLoaderController("/BangLuongNV/LoadBangLuongNVDieuChinh/", total, page, "?qsearch=" + qSearch);
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangLuongNhanVien_DieuChinhLuong(maPhongBan, thang, nam, qSearch).Skip(start).Take(offset).ToList();

            ViewData["qSearchLuong"] = qSearch;
            return PartialView("_LoadBangLuongNVDieuChinh");
        }
        public ActionResult ViewChiTietLuong(string thang, string nam, string maNhanVien)
        {
            #region Role user
            permission = GetPermission("XemBangLuong", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            var dsMauIn = nhanSuContext.GetTable<BatDongSan.Models.HeThong.Sys_PrintTemplate>().Where(d => d.maMauIn == "MIBLNV").FirstOrDefault();
            if (nam == "2018")
            {
                dsMauIn = nhanSuContext.GetTable<BatDongSan.Models.HeThong.Sys_PrintTemplate>().Where(d => d.maMauIn == "MIBLNV2018").FirstOrDefault();
            }
            string noiDung = string.Empty;

            var ds = nhanSuContext.tbl_NS_BangLuongNhanViens
                               .Where(t => t.maNhanVien == maNhanVien && t.thang.ToString() == thang && nam == t.nam.ToString()).FirstOrDefault();

            var hinhThucLuong = nhanSuContext.tbl_NS_HinhThucLuongs.Where(d => d.maHinhThuc == ds.hinhThucLuong).Select(d => d.tenHinhThuc).FirstOrDefault() ?? String.Empty;

            ViewData["chiTiet"] = dsMauIn;
            if (dsMauIn != null)
            {
                var LuongTheoNgayCong = ds.luongThang ?? 0;
                var TongTienPhuCap = (ds.phuCapTienAn ?? 0) + (ds.phuCapDienThoai ?? 0) + (ds.phuCapThuHut ?? 0) + (Convert.ToDouble(ds.phuCapDiLaiNew ?? 0));
                var TongThuNhap = LuongTheoNgayCong + TongTienPhuCap + (ds.congTacPhi ?? 0) + (ds.sinhHoatPhi ?? 0);
                var TongKhauTru = (ds.baoHiemXH ?? 0) + (ds.baoHiemYTe ?? 0) + (ds.baoHiemTN ?? 0) + (ds.thue ?? 0) + (ds.doanPhi ?? 0);
                if (ds.phuCapDiLai == 2)
                {
                    var TruBaoHiem = (ds.baoHiemXH ?? 0) + (ds.baoHiemYTe ?? 0) + (ds.baoHiemTN ?? 0);
                    TongKhauTru = TongKhauTru - TruBaoHiem;
                }
                if (ds.phuCapTienXang == 2)
                {
                    TongKhauTru = TongKhauTru - (ds.thue ?? 0);
                }

                var TongPhuCapTL = (ds.truyLanh ?? 0) - (ds.truyThu ?? 0) - (ds.dangPhi ?? 0) + (ds.phuCapKhac ?? 0) + (ds.luongThanhTich1 ?? 0);//+ (ds.soNgayPhepTang ?? 0) - (ds.luongThanhTich3 ?? 0);
                var TongThucLanh = TongThuNhap - TongKhauTru + TongPhuCapTL;
                var soNgayTangCa = (ds.soNgayTangCaThuong ?? 0) + (ds.soNgayTangCaNgayLe ?? 0) + (ds.soNgayTangCaChuNhat ?? 0);
                noiDung = dsMauIn.html.Replace("{$thang}", Convert.ToString(thang))
                    .Replace("{$nam}", Convert.ToString(nam))
                    .Replace("{$hoVaTen}", ds.hoTen)
                    //capBacChucVu 
                    .Replace("{$capBacChucVu}", ds.chucVu)
                    .Replace("{$phongBan}", ds.phongBan)
                    .Replace("{$bac}", Convert.ToString(ds.bac))
                    .Replace("{$hinhThucLuong}", Convert.ToString(hinhThucLuong))
                    //A. Tổng Lương 
                    .Replace("{$tongLuong}", String.Format("{0:###,##0}", ds.tongLuong ?? 0))
                    .Replace("{$luongCoBan}", String.Format("{0:###,##0}", ds.phuCapLuong ?? 0))
                    .Replace("{$luongThanhTich}", String.Format("{0:###,##0}", ds.khoanBoSungLuong ?? 0))
                    .Replace("{$luongChoViec}", String.Format("{0:###,##0}", ds.luongChoViec ?? 0))
                    //B. Ngày Công
                    .Replace("{$ngayCongChuan}", Convert.ToString(ds.ngayCongChuan ?? 0))
                    .Replace("{$ngayCongTinhLuong}", Convert.ToString(ds.tongNgayCong ?? 0))
                    .Replace("{$ngayCong}", Convert.ToString(ds.soNgayQuet ?? 0))
                    .Replace("{$ngayCongTac}", Convert.ToString(ds.soNgayCongTac ?? 0))
                    .Replace("{$nghiPhep}", Convert.ToString(ds.soNgayNghiPhep ?? 0))
                    .Replace("{$nghiBu}", Convert.ToString(ds.soNgayNghiBu ?? 0))
                    .Replace("{$nghiLeTet}", Convert.ToString(ds.soNgayNghiLe ?? 0))
                    .Replace("{$ngayNghiKhongLuong}", Convert.ToString(ds.soNgayNghiKhongLuong ?? 0))
                    .Replace("{$soNgayTangCaThuong+soNgayTangCaLe+soNgayTangCaChuNhat}", Convert.ToString(soNgayTangCa))
                    //C. Lương Theo Ngày Công (A1*B2/B1)
                    .Replace("{$luongTheoNgayCong}", String.Format("{0:###,##0}", ds.luongThang ?? 0))
                    //Chờ Việc
                    //.Replace("{$luongChoViec}", String.Format("{0:###,##0}", ds.luongChoViec??0))
                    //.Replace("{$congChoViec}", Convert.ToString(ds.soNgayCongChoViec??0))
                    //.Replace("{$luongChoViec}", String.Format("{0:###,##0}", ds.luongChoViec??0))
                    // Các Khoản Phụ Cấp
                    .Replace("{$tongTienPhuCap}", String.Format("{0:###,##0}", TongTienPhuCap))
                    .Replace("{$tienAnGiuaCa}", String.Format("{0:###,##0}", ds.phuCapTienAn ?? 0))
                    .Replace("{$tienDienThoai}", String.Format("{0:###,##0}", ds.phuCapDienThoai ?? 0))
                    .Replace("{$tienThuHut}", String.Format("{0:###,##0}", ds.phuCapThuHut ?? 0))
                    .Replace("{$phuCapDiLai}", String.Format("{0:###,##0}", ds.phuCapDiLaiNew ?? 0))
                    .Replace("{$congTacPhi}", String.Format("{0:###,##0}", ds.congTacPhi ?? 0))
                    .Replace("{$sinhHoatPhi}", String.Format("{0:###,##0}", ds.sinhHoatPhi ?? 0))
                    .Replace("{$dangPhi}", String.Format("{0:###,##0}", ds.dangPhi ?? 0))
                    .Replace("{$soNgayCongChoViec}", String.Format("{0:###,##0}", ds.soNgayCongChoViec ?? 0))
                    //tongThuNhap=   Lương Theo Ngày Công (A1*B2/B1) +  Các Khoản Phụ Cấp
                     .Replace("{$tongThuNhap}", String.Format("{0:###,##0}", TongThuNhap))
                    //tongThucLanh=  Tổng Thu Nhập(C+D) 
                     .Replace("{$tongThucLanh}", String.Format("{0:###,##0}", TongThucLanh))//(ds.thucLanh ?? 0) + (ds.tamUng ?? 0) + (ds.luongThanhTich2 ?? 0) + (ds.luongThanhTich3 ?? 0)))
                    // Các Khoản Khấu Trừ

                    .Replace("{$tongKhauTru}", String.Format("{0:###,##0}", TongKhauTru))
                    .Replace("{$tienBHXH}", String.Format("{0:###,##0}", ds.baoHiemXH ?? 0))
                    .Replace("{$tienBHYT}", String.Format("{0:###,##0}", ds.baoHiemYTe ?? 0))
                    .Replace("{$tienBHTN}", String.Format("{0:###,##0}", ds.baoHiemTN ?? 0))
                    .Replace("{$tienTTNCT}", String.Format("{0:###,##0}", ds.thue ?? 0))
                    .Replace("{$tienDoanPhi}", String.Format("{0:###,##0}", ds.doanPhi ?? 0))
                    //Phụ Cấp Khác Và Truy Lĩnh
                    .Replace("{$tongPhuCapTL}", String.Format("{0:###,##0}", TongPhuCapTL))
                    .Replace("{$tienPhuCap}", String.Format("{0:###,##0}", ds.phuCapKhac ?? 0))
                    .Replace("{$tienTTTLThue}", String.Format("{0:###,##0}", ds.soNgayPhepTang ?? 0))
                    .Replace("{$tienTruyThu}", String.Format("{0:###,##0}", ds.truyThu ?? 0))
                    .Replace("{$tienTruyLanh}", String.Format("{0:###,##0}", ds.truyLanh ?? 0))
                    //	Truy lãnh tiền cơm
                    .Replace("{$luongtt1}", String.Format("{0:###,##0}", ds.luongThanhTich1 ?? 0))
                    //	Truy thu/truy lãnh thuế TNCN(Quyết toán thuế 2016)
                    //.Replace("{$TTTLthueTNCN}", String.Format("{0:###,##0}", ds.TTTLThue ?? 0))
                    //Tạm Ứng
                     .Replace("{$tamUng}", String.Format("{0:###,##0}", ds.tamUng ?? 0))
                    //Thực Nhận Lương Tháng (C+D+E-F+G-H)
                    .Replace("{$tongThucNhanLuong}", String.Format("{0:###,##0}", (ds.thucLanh ?? 0)))
                .Replace("{$quyetToanLuongT1}", String.Format("{0:###,##0}", ds.luongThanhTich2 ?? 0))
                .Replace("{$truyThuThueThuongTTT13_2016}", String.Format("{0:###,##0}", ds.luongThanhTich3 ?? 0))
                .Replace("{$luongThanhTich3}", String.Format("{0:###,##0}", ds.luongThanhTich3 ?? 0))
                .Replace("{$soNgayPhepTang}", String.Format("{0:###,##0}", ds.soNgayPhepTang ?? 0))
                .Replace("{$truyLanhThangTruoc}", String.Format("{0:###,##0}", ds.truyLanhThangTruoc ?? 0));
            }
            ViewBag.NoiDung = noiDung;
            // return PartialView("_ViewChiTietLuong");
            return PartialView("_ViewChiTietLuongTemplate");
        }


        public ActionResult LoadBangLuongTheoBP(int thang, int nam, int _page = 0)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            int page = _page == 0 ? 1 : _page;
            int pIndex = page;
            //int total = nhanSuContext.sp_NS_BangLuongTheoBoPhan(thang, nam).Count();
            //PagingLoaderController("/BangLuongNV/LoadBangLuongTheoBP/", total, page, "?qsearch=" + null);
            //ViewData["lsDanhSachBP"] = nhanSuContext.sp_NS_BangLuongTheoBoPhan(thang, nam).Skip(start).Take(offset).ToList();

            string[] maKhoiTinhLuong = nhanSuContext.tbl_NS_BangLuongNhanViens.Where(d => d.thang == thang && d.nam == nam).Select(d => (d.maKhoiDeNghiChiLuong ?? "KTDNCL240708-001")).Distinct().ToArray();
            var listKTL = nhanSuContext.GetTable<tbl_NS_KhoiTaoDeNghiChiLuong>().Where(d => maKhoiTinhLuong.Contains(d.maKhoiDeNghiChiLuong)).ToList();
            for (int i = 0; i < listKTL.Count(); i++)
            {
                ViewData["lstKTL" + i + ""] = nhanSuContext.sp_NS_BangLuongTheoBoPhan(thang, nam, listKTL[i].maKhoiDeNghiChiLuong).ToList();
            }
            ViewBag.ListKTL = listKTL;
            return PartialView("_LoadBangLuongTheoBP");
        }
        public ActionResult BangLuongNV()
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            buildTree = new StringBuilder();
            phongBans = nhanSuContext.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan>().ToList();
            buildTree = TreePhongBanAjax.BuildTreeDepartment(phongBans);
            ViewBag.PhongBans = buildTree.ToString();
            thang(DateTime.Now.Month);
            nam(DateTime.Now.Year);
            return View("");

        }
        public ActionResult BangLuongChuyenNN()
        {
            #region Role user
            permission = GetPermission("BangLuongNVNN", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            buildTree = new StringBuilder();
            phongBans = nhanSuContext.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan>().ToList();
            buildTree = TreePhongBanAjax.BuildTreeDepartment(phongBans);
            ViewBag.PhongBans = buildTree.ToString();
            thang(DateTime.Now.Month);
            nam(DateTime.Now.Year);
            return View("");

        }

        public ActionResult LoadBangLuongChuyenNN(string maPhongBan, string qSearch, int thang, int nam, int _page = 0)
        {
            #region Role user
            permission = GetPermission("BangLuongNVNN", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            int page = _page == 0 ? 1 : _page;
            int pIndex = page;
            int total = nhanSuContext.sp_NS_BangLuongChuyenNganHang(thang, nam, maPhongBan, qSearch).Count();
            PagingLoaderController("/BangLuongNV/BangLuongChuyenNN/", total, page, "?qsearch=" + qSearch);
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangLuongChuyenNganHang(thang, nam, maPhongBan, qSearch).Skip(start).Take(offset).ToList();

            ViewData["qSearch"] = qSearch;
            return PartialView("_LoadBangLuongChuyenNN");
        }

        public ActionResult UpdateLuongNV(int thang, int nam)
        {
            try
            {
                #region Role user
                permission = GetPermission(MCV, BangPhanQuyen.QuyenSua);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion
                //Check 
                var result = new { kq = false };
                var checkEx = nhanSuContext.tbl_DuyetBangLuongNVs.Where(t => t.nam == nam && t.thang == thang).FirstOrDefault();
                if (checkEx != null)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                //End check
                var list = nhanSuContext.sp_NS_TinhLuongNhanVien(thang, nam);
                result = new { kq = true };
                SaveActiveHistory("Tính lương nhân viên tháng: " + thang + " năm: " + nam);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return View();
            }

        }
        public ActionResult UpdateLuongNVTinhLai(int thang, int nam)
        {
            try
            {
                #region Role user
                permission = GetPermission(MCV, BangPhanQuyen.QuyenSua);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion

                var list = nhanSuContext.sp_NS_TinhLuongNhanVien_TinhLaiLuong(thang, nam);
                var result = new { kq = true };
                SaveActiveHistory("Tính lương nhân viên, tính lại tháng: " + thang + " năm: " + nam);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return View();
            }

        }
        public ActionResult DuyetLuongNV(int thang, int nam)
        {


            try
            {
                #region Role user
                permission = GetPermission(MCV, BangPhanQuyen.QuyenDuyet);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion
                //Check 
                var result = new { kq = false };
                var checkEx = nhanSuContext.tbl_DuyetBangLuongNVs.Where(t => t.nam == nam && t.thang == thang).FirstOrDefault();
                if (checkEx != null)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                //End check
                // Insert Row 
                tbl_DuyetBangLuongNV tblDuyetBL = new tbl_DuyetBangLuongNV();
                tblDuyetBL.nam = nam;
                tblDuyetBL.thang = thang;
                tblDuyetBL.ngayDuyet = DateTime.Now;
                tblDuyetBL.nguoiDuyet = GetUser().manv;
                nhanSuContext.tbl_DuyetBangLuongNVs.InsertOnSubmit(tblDuyetBL);
                nhanSuContext.SubmitChanges();
                // End Insert Row
                // Check Exist
                var list = nhanSuContext.tbl_DuyetBangLuongNVs.Where(t => t.nam == nam && t.thang == thang).FirstOrDefault();

                if (list != null)
                {
                    result = new { kq = true };
                }
                SaveActiveHistory("Duyệt lương nhân viên tháng: " + thang + " năm: " + nam);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return View();
            }

        }
        public ActionResult SendBangLuongNV(int thang, int nam)
        {

            string maPhongBan = "";
            string qSearch = "";
            var list = nhanSuContext.tbl_DuyetBangLuongNVs.Where(t => t.nam == nam && t.thang == thang).FirstOrDefault();
            var result = new { kq = false };
            if (list == null)
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            var listSendMails = nhanSuContext.sp_NS_BangLuongNhanVien(maPhongBan, thang, nam, qSearch).ToList();
            foreach (var ds in listSendMails)
            {
                var dsMauIn = nhanSuContext.GetTable<BatDongSan.Models.HeThong.Sys_PrintTemplate>().Where(d => d.maMauIn == "MIBLNV").FirstOrDefault();
                if (nam == 2018)
                {
                    dsMauIn = nhanSuContext.GetTable<BatDongSan.Models.HeThong.Sys_PrintTemplate>().Where(d => d.maMauIn == "MIBLNV2018").FirstOrDefault();
                }
                string noiDung = string.Empty;
                var hinhThucLuong = nhanSuContext.tbl_NS_HinhThucLuongs.Where(d => d.maHinhThuc == ds.hinhThucLuong).Select(d => d.tenHinhThuc).FirstOrDefault() ?? String.Empty;
                //Replace bang luong send mail
                var LuongTheoNgayCong = ds.luongThang ?? 0;
                var TongTienPhuCap = (ds.phuCapTienAn ?? 0) + (ds.phuCapDienThoai ?? 0) + (ds.phuCapThuHut ?? 0) + (Convert.ToDouble(ds.phuCapDiLaiNew ?? 0)) + (ds.sinhHoatPhi ?? 0);
                var TongThuNhap = LuongTheoNgayCong + TongTienPhuCap + (ds.congTacPhi ?? 0);
                var TongKhauTru = (ds.baoHiemXH ?? 0) + (ds.baoHiemYTe ?? 0) + (ds.baoHiemTN ?? 0) + (ds.thue ?? 0) + (ds.doanPhi ?? 0);
                if (ds.phuCapDiLai == 2)
                {
                    var TruBaoHiem = (ds.baoHiemXH ?? 0) + (ds.baoHiemYTe ?? 0) + (ds.baoHiemTN ?? 0);
                    TongKhauTru = TongKhauTru - TruBaoHiem;
                }
                if (ds.phuCapTienXang == 2)
                {
                    TongKhauTru = TongKhauTru - (ds.thue ?? 0);
                }
                var TongPhuCapTL = (ds.truyLanh ?? 0) - (ds.truyThu ?? 0) - (ds.dangPhi ?? 0) + (ds.phuCapKhac ?? 0) + (ds.luongThanhTich1 ?? 0) + (ds.soNgayPhepTang ?? 0);
                var TongThucLanh = TongThuNhap - TongKhauTru + TongPhuCapTL;
                var soNgayTangCa = (ds.soNgayTangCaThuong ?? 0) + (ds.soNgayTangCaNgayLe ?? 0) + (ds.soNgayTangCaChuNhat ?? 0);
                noiDung = dsMauIn.html.Replace("{$thang}", Convert.ToString(thang))
                    .Replace("{$nam}", Convert.ToString(nam))
                    .Replace("{$hoVaTen}", ds.hoTen)
                    //capBacChucVu 
                    .Replace("{$capBacChucVu}", ds.chucVu)
                    .Replace("{$phongBan}", ds.phongBan)
                    .Replace("{$bac}", Convert.ToString(ds.bac))
                    .Replace("{$hinhThucLuong}", Convert.ToString(hinhThucLuong))
                    //A. Tổng Lương 
                    .Replace("{$tongLuong}", String.Format("{0:###,##0}", ds.tongLuong ?? 0))
                    .Replace("{$luongCoBan}", String.Format("{0:###,##0}", ds.phuCapLuong ?? 0))
                    .Replace("{$luongThanhTich}", String.Format("{0:###,##0}", ds.khoanBoSungLuong ?? 0))
                    .Replace("{$luongChoViec}", String.Format("{0:###,##0}", ds.luongChoViec ?? 0))
                    //B. Ngày Công
                    .Replace("{$ngayCongChuan}", Convert.ToString(ds.ngayCongChuan ?? 0))
                    .Replace("{$ngayCongTinhLuong}", Convert.ToString(ds.tongNgayCong ?? 0))
                    .Replace("{$ngayCong}", Convert.ToString(ds.soNgayQuet ?? 0))
                    .Replace("{$ngayCongTac}", Convert.ToString(ds.soNgayCongTac ?? 0))
                    .Replace("{$nghiPhep}", Convert.ToString(ds.soNgayNghiPhep ?? 0))
                    .Replace("{$nghiBu}", Convert.ToString(ds.soNgayNghiBu ?? 0))
                    .Replace("{$nghiLeTet}", Convert.ToString(ds.soNgayNghiLe ?? 0))
                    .Replace("{$ngayNghiKhongLuong}", Convert.ToString(ds.soNgayNghiKhongLuong ?? 0))
                    .Replace("{$soNgayTangCaThuong+soNgayTangCaLe+soNgayTangCaChuNhat}", Convert.ToString(soNgayTangCa))
                    //C. Lương Theo Ngày Công (A1*B2/B1)
                    .Replace("{$luongTheoNgayCong}", String.Format("{0:###,##0}", ds.luongThang ?? 0))
                    //Chờ Việc
                    //.Replace("{$luongChoViec}", String.Format("{0:###,##0}", ds.luongChoViec??0))
                    //.Replace("{$congChoViec}", Convert.ToString(ds.soNgayCongChoViec??0))
                    //.Replace("{$luongChoViec}", String.Format("{0:###,##0}", ds.luongChoViec??0))
                    // Các Khoản Phụ Cấp
                    .Replace("{$tongTienPhuCap}", String.Format("{0:###,##0}", TongTienPhuCap))
                    .Replace("{$tienAnGiuaCa}", String.Format("{0:###,##0}", ds.phuCapTienAn ?? 0))
                    .Replace("{$tienDienThoai}", String.Format("{0:###,##0}", ds.phuCapDienThoai ?? 0))
                    .Replace("{$tienThuHut}", String.Format("{0:###,##0}", ds.phuCapThuHut ?? 0))
                    .Replace("{$phuCapDiLai}", String.Format("{0:###,##0}", ds.phuCapDiLaiNew ?? 0))
                    .Replace("{$congTacPhi}", String.Format("{0:###,##0}", ds.congTacPhi ?? 0))
                    .Replace("{$sinhHoatPhi}", String.Format("{0:###,##0}", ds.sinhHoatPhi ?? 0))
                    .Replace("{$dangPhi}", String.Format("{0:###,##0}", ds.dangPhi ?? 0))
                    .Replace("{$soNgayCongChoViec}", String.Format("{0:###,##0}", ds.soNgayCongChoViec ?? 0))
                    //tongThuNhap=   Lương Theo Ngày Công (A1*B2/B1) +  Các Khoản Phụ Cấp
                     .Replace("{$tongThuNhap}", String.Format("{0:###,##0}", TongThuNhap))
                    //tongThucLanh=  Tổng Thu Nhập(C+D) 
                     .Replace("{$tongThucLanh}", String.Format("{0:###,##0}", TongThucLanh))
                    // Các Khoản Khấu Trừ

                    .Replace("{$tongKhauTru}", String.Format("{0:###,##0}", TongKhauTru))
                    .Replace("{$tienBHXH}", String.Format("{0:###,##0}", ds.baoHiemXH ?? 0))
                    .Replace("{$tienBHYT}", String.Format("{0:###,##0}", ds.baoHiemYTe ?? 0))
                    .Replace("{$tienBHTN}", String.Format("{0:###,##0}", ds.baoHiemTN ?? 0))
                    .Replace("{$tienTTNCT}", String.Format("{0:###,##0}", ds.thue ?? 0))
                    .Replace("{$tienDoanPhi}", String.Format("{0:###,##0}", ds.doanPhi ?? 0))
                    //Phụ Cấp Khác Và Truy Lĩnh
                    .Replace("{$tongPhuCapTL}", String.Format("{0:###,##0}", TongPhuCapTL))
                    .Replace("{$tienPhuCap}", String.Format("{0:###,##0}", ds.phuCapKhac ?? 0))
                    .Replace("{$tienTTTLThue}", String.Format("{0:###,##0}", ds.soNgayPhepTang ?? 0))
                    .Replace("{$tienTruyThu}", String.Format("{0:###,##0}", ds.truyThu ?? 0))
                    .Replace("{$tienTruyLanh}", String.Format("{0:###,##0}", ds.truyLanh ?? 0))
                    .Replace("{$luongtt1}", String.Format("{0:###,##0}", ds.luongThanhTich1 ?? 0))
                    //Tạm Ứng
                     .Replace("{$tamUng}", String.Format("{0:###,##0}", ds.tamUng ?? 0))
                    //Thực Nhận Lương Tháng (C+D+E-F+G-H)
                    .Replace("{$tongThucNhanLuong}", String.Format("{0:###,##0}", ds.thucLanh ?? 0))
                    .Replace("{$quyetToanLuongT1}", String.Format("{0:###,##0}", ds.luongThanhTich2 ?? 0))
                .Replace("{$truyThuThueThuongTTT13_2016}", String.Format("{0:###,##0}", ds.luongThanhTich3 ?? 0));
                //End
                // Code send mail
                MailHelper mailInit = new MailHelper(); // lay cac tham so trong webconfig
                System.Text.StringBuilder content = new System.Text.StringBuilder();

                content.Append("<h3>Email từ hệ thống nhân sự</h3>");
                content.Append("<p>Xin chào: " + ds.hoTen + " !</p>");
                //Content
                content.Append(noiDung);

                //End content
                content.Append("<p style='font-style:italic'>Thanks and Regards!</p>");
                //Send only email is @thuanviet.com.vn
                string[] array01 = ds.email.ToLower().Split('@');
                //string string2 = ConfigurationManager.AppSettings["OnlySend"]; //get domain from config files
                //string[] array1 = string2.Split(',');
                // bool EmailofThuanViet;
                //EmailofThuanViet = array1.Contains(array01[1]);
                // if (emailNV == "" || emailNV == null || EmailofThuanViet == false)
                // {
                //    return false;
                // }
                MailAddress toMail = new MailAddress(ds.email, ds.hoTen); // goi den mail
                mailInit.ToMail = toMail;
                mailInit.Body = content.ToString();
                mailInit.SendMail();
                // End code send mail
            }
            result = new { kq = true };
            SaveActiveHistory("Send mail bảng lương nhân viên: " + thang + " năm: " + nam);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        private void thang(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 0; i < 13; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["thang"] = new SelectList(dics, "Key", "Value", value);
            ViewData["thangBP"] = new SelectList(dics, "Key", "Value", value);
            ViewData["thangLuong"] = new SelectList(dics, "Key", "Value", value);
            ViewData["thangTinhLai"] = new SelectList(dics, "Key", "Value", value);

        }
        private void nam(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 2015; i < 2031; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["nam"] = new SelectList(dics, "Key", "Value", value);
            ViewData["namBP"] = new SelectList(dics, "Key", "Value", value);
            ViewData["namLuong"] = new SelectList(dics, "Key", "Value", value);
            ViewData["namTinhLai"] = new SelectList(dics, "Key", "Value", value);

        }
        #region Cây sơ đồ tổ chức nhân viên và phòng ban

        //Load danh sach nhan vien khi click button add them:
        public ActionResult DanhSachNhanVien()
        {
            try
            {
                LinqDanhMucDataContext linqDM = new LinqDanhMucDataContext();
                buildTree = new StringBuilder();
                phongBans = linqDM.tbl_DM_PhongBans.ToList();
                buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
                ViewBag.departments = buildTree.ToString();
                ViewBag.shiftType = linqDM.tbl_NS_PhanCas.Where(t => t.tenPhanCa != "").ToList();
                ViewBag.page = 0;
                ViewBag.total = 0;

                return View(phongBans);
            }
            catch (Exception ex)
            {

                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }
        public ActionResult LoadNhanVien(string qSearch, int _page, string parrentId)
        {
            try
            {
                LinqDanhMucDataContext linqDM = new LinqDanhMucDataContext();
                string parentID = linqDM.tbl_DM_PhongBans.Where(d => d.maPhongBan == parrentId).Select(d => d.maPhongBan).FirstOrDefault() ?? string.Empty;
                if (String.IsNullOrEmpty(parentID))
                {
                    parrentId = string.Empty;
                }
                int page = _page == 0 ? 1 : _page;
                int pIndex = page;
                int total = linqDM.sp_PB_DanhSachNhanVien(qSearch, parrentId).Count();
                PagingLoaderController("/BangLuongNV/Index/", total, page, "?qsearch=" + qSearch + "&parrentId=" + parrentId);
                ViewBag.nhanVien = linqDM.sp_PB_DanhSachNhanVien(qSearch, parrentId).Skip(start).Take(offset).ToList();
                ViewBag.parrentId = parrentId;
                ViewBag.qSearch = qSearch ?? string.Empty;
                ViewBag.currentMaNV = GetUser().manv;
                return PartialView("LoadNhanVien");
            }
            catch (Exception e)
            {
                ViewData["Message"] = e.Message;
                return View("Error");
            }
        }
        public JsonResult UpdateDanhSachTCLuong(FormCollection form)
        {
            try
            {
                int thangLuong = Convert.ToInt32(form["thangLuong"]);
                int namLuong = Convert.ToInt32(form["namLuong"]);
                var deleteLuong = nhanSuContext.tbl_NS_BangLuongNhanVien_DieuChinhLuongs.Where(d => d.thang == thangLuong && d.nam == namLuong).ToList();
                if (deleteLuong != null && deleteLuong.Count > 0)
                {
                    nhanSuContext.tbl_NS_BangLuongNhanVien_DieuChinhLuongs.DeleteAllOnSubmit(deleteLuong);
                }
                List<tbl_NS_BangLuongNhanVien_DieuChinhLuong> lstBangLuong = new List<tbl_NS_BangLuongNhanVien_DieuChinhLuong>();
                tbl_NS_BangLuongNhanVien_DieuChinhLuong tblBangLuong;
                var maNhanVien = form.GetValues("maNhanVien");
                if (maNhanVien != null)
                {
                    for (int i = 0; i < maNhanVien.Length; i++)
                    {
                        var tenOption = maNhanVien[i] + "optradio";
                        tblBangLuong = new tbl_NS_BangLuongNhanVien_DieuChinhLuong();
                        tblBangLuong.nam = namLuong;
                        tblBangLuong.thang = thangLuong;
                        tblBangLuong.maNhanVien = maNhanVien[i];
                        tblBangLuong.loaiTinh = Convert.ToInt32(form[tenOption]);
                        tblBangLuong.ngayTao = DateTime.Now;
                        tblBangLuong.nguoiTao = GetUser().manv;
                        lstBangLuong.Add(tblBangLuong);
                    }
                    nhanSuContext.tbl_NS_BangLuongNhanVien_DieuChinhLuongs.InsertAllOnSubmit(lstBangLuong);
                }
                nhanSuContext.SubmitChanges();
            }
            catch
            {

            }
            return Json("OK");

        }
        public string InsertDanhSachFullCong(string MaNV, int thang, int nam)
        {

            try
            {
                // Update
                string[] MaNVs = MaNV.Split(',');
                List<tbl_NS_BangLuongNhanVien_DieuChinhLuong> lstBangLuong = new List<tbl_NS_BangLuongNhanVien_DieuChinhLuong>();
                tbl_NS_BangLuongNhanVien_DieuChinhLuong tblBangLuong;
                for (int i = 0; i < MaNVs.Length; i++)
                {
                    tblBangLuong = new tbl_NS_BangLuongNhanVien_DieuChinhLuong();
                    tblBangLuong.nam = nam;
                    tblBangLuong.thang = thang;
                    tblBangLuong.maNhanVien = MaNVs[i];
                    tblBangLuong.loaiTinh = 1;
                    tblBangLuong.ngayTao = DateTime.Now;
                    tblBangLuong.nguoiTao = GetUser().manv;
                    lstBangLuong.Add(tblBangLuong);


                }

                nhanSuContext.tbl_NS_BangLuongNhanVien_DieuChinhLuongs.InsertAllOnSubmit(lstBangLuong);
                nhanSuContext.SubmitChanges();
                return "true";


            }
            catch (Exception ex)
            {
                return "Lỗi: " + ex.ToString();
            }
        }
        public string DeleteDanhSachTCLuong(string maNhanVien, int thang, int nam)
        {

            try
            {
                var tblCong = nhanSuContext.tbl_NS_BangLuongNhanVien_DieuChinhLuongs.Where(d => d.maNhanVien == maNhanVien && d.thang == thang && d.nam == nam).FirstOrDefault();
                if (tblCong != null)
                {
                    nhanSuContext.tbl_NS_BangLuongNhanVien_DieuChinhLuongs.DeleteOnSubmit(tblCong);
                    nhanSuContext.SubmitChanges();
                }
                return "true";


            }
            catch (Exception ex)
            {
                return "Lỗi: " + ex.ToString();
            }
        }
        public string UpdateNhanVienFullCong(string maNhanVien, int thang, int nam, int loai)
        {

            try
            {
                var tblCong = nhanSuContext.tbl_NS_BangLuongNhanVien_DieuChinhLuongs.Where(d => d.maNhanVien == maNhanVien && d.thang == thang && d.nam == nam).FirstOrDefault();
                if (tblCong != null)
                {
                    tblCong.loaiTinh = loai;
                    nhanSuContext.SubmitChanges();
                }
                return "true";


            }
            catch (Exception ex)
            {
                return "Lỗi: " + ex.ToString();
            }
        }

        #endregion
        // Xuat File Bang Luong Nhan Vien
        // Xuat File Bang Luong Nhan Vien
        public void XuatFileBLNV(int thang, int nam, string maPhongBan, string qSearch)
        {

            try
            {
                #region Role user
                permission = GetPermission(MCV, BangPhanQuyen.QuyenIn);
                if (!permission.HasValue)
                    return;
                if (!permission.Value)
                    return;
                #endregion

                var filename = "";
                var virtualPath = HttpRuntime.AppDomainAppVirtualPath;

                var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplate.xls"), FileMode.Open, FileAccess.Read);

                var workbook = new HSSFWorkbook(fileStream, true);
                filename += "BangLuongNV_" + nam + "_" + thang + ".xls";


                var sheet = workbook.GetSheet("danhsachnhanvien");


                #region format style excel
                /*style title start*/
                //tạo font cho các title
                //font tiêu đề 
                HSSFFont hFontTieuDe = (HSSFFont)workbook.CreateFont();
                hFontTieuDe.FontHeightInPoints = 18;
                hFontTieuDe.Boldweight = 100 * 10;
                hFontTieuDe.FontName = "Times New Roman";
                hFontTieuDe.Color = HSSFColor.BLUE.index;

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
                //font chứ hoa đậm
                HSSFFont hFontNommalUpperRED = (HSSFFont)workbook.CreateFont();
                hFontNommalUpperRED.Boldweight = (short)FontBoldWeight.BOLD;
                hFontNommalUpperRED.Color = HSSFColor.RED.index;
                hFontNommalUpperRED.FontName = "Times New Roman";

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

                //style sum cell
                var styleCellSumary = workbook.CreateCellStyle();
                styleCellSumary.SetFont(hFontNommalUpper);
                styleCellSumary.WrapText = true;
                styleCellSumary.BorderBottom = CellBorderType.THIN;
                styleCellSumary.BorderLeft = CellBorderType.THIN;
                styleCellSumary.BorderRight = CellBorderType.THIN;
                styleCellSumary.BorderTop = CellBorderType.THIN;
                styleCellSumary.VerticalAlignment = VerticalAlignment.CENTER;
                styleCellSumary.Alignment = HorizontalAlignment.RIGHT;
                //style sum cell
                var styleCellSumaryTen = workbook.CreateCellStyle();
                styleCellSumaryTen.SetFont(hFontNommalUpper);
                styleCellSumaryTen.WrapText = true;
                styleCellSumaryTen.BorderBottom = CellBorderType.THIN;
                styleCellSumaryTen.BorderLeft = CellBorderType.THIN;
                styleCellSumaryTen.BorderRight = CellBorderType.THIN;
                styleCellSumaryTen.BorderTop = CellBorderType.THIN;
                styleCellSumaryTen.VerticalAlignment = VerticalAlignment.CENTER;
                styleCellSumaryTen.Alignment = HorizontalAlignment.LEFT;

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

                string cellTenCty = "CÔNG TY CỔ PHẦN TV.WINDOW";
                var titleCellCty = HSSFCellUtil.CreateCell(sheet.CreateRow(0), 0, cellTenCty.ToUpper());
                titleCellCty.CellStyle = styleTitle;

                //string cellEnd1 = "Bảng Lương Tháng " + thang + " năm " + nam;
                //var titleCellEnd1 = HSSFCellUtil.CreateCell(sheet.GetRow(0), 43, cellEnd1.ToUpper());
                //titleCellEnd1.CellStyle = styleTitle;

                string rowtitle = "Bảng Lương Công Ty Tháng " + thang + " năm " + nam;
                var titleCell = HSSFCellUtil.CreateCell(sheet.CreateRow(firstRowNumber), 15, rowtitle.ToUpper());
                titleCell.CellStyle = styleTitle;

                //string cellEnd2 = "";
                //var titleCellEnd2 = HSSFCellUtil.CreateCell(sheet.GetRow(1), 43, cellEnd2.ToUpper());
                //titleCellEnd2.CellStyle = styleTitle;

                ++firstRowNumber;

                var list1 = new List<string>();
                list1.Add("STT");
                list1.Add("Mã nhân viên");
                list1.Add("Họ tên");
                list1.Add("Công ty");
                list1.Add("Chức vụ");
                list1.Add("Phòng ban");
                list1.Add("Bậc");
                list1.Add("Mức");
                list1.Add("Tổng lương");
                list1.Add("Lương cơ bản");
                list1.Add("Lương thành tích");
                list1.Add("Lương kiêm nhiệm");
                list1.Add("Lương đóng BHXH");
                list1.Add("Ngày công chuẩn");

                list1.Add("Ngày công tính lương");
                list1.Add("Ngày nghỉ");// 
                list1.Add("");//
                list1.Add("");//
                list1.Add("");//
                list1.Add("");//
                list1.Add("Tăng ca");//
                list1.Add("");//
                list1.Add("");   //         
                list1.Add("Lương CB theo ngày công thực tế");
                list1.Add("Các khoản phụ cấp");
                list1.Add("");//
                list1.Add("");//
                list1.Add("");//
                list1.Add("");//
                list1.Add("");//

                list1.Add("Phụ cấp khác");
                list1.Add("Công tác phí");

                list1.Add("Truy lãnh");
                list1.Add("Thu nhập khác");
                list1.Add("Tiền cơm công tác");
                list1.Add("Tổng thu nhập");

                list1.Add("Tổng thu nhập chịu thuế");

                list1.Add("Số người phụ thuộc");
                list1.Add("Các khoản giảm trừ không tính thuế TNCN");
                list1.Add("");
                list1.Add("Thu nhập tính thuế TNCN");
                list1.Add("Các khoản trích trừ vào lương");
                list1.Add("");
                list1.Add("");
                list1.Add("Quỹ nội bộ (1%)");
                list1.Add("Thuế TNCN (Tạm thu)");
                list1.Add("Truy thu");

                list1.Add("Truy thu bảo hiểm");
                list1.Add("Tổng thu nhập thực tế");

                list1.Add("Tạm ứng");
                //list1.Add("Quyết toán lương tháng 1");
                list1.Add("Truy thu thuế TNCN 2017");
                list1.Add("Truy lãnh thuế TNCN 2017");
                list1.Add("THỰC LÃNH");
                list1.Add("DN đóng BHXH");
                list1.Add("");
                list1.Add("Ngày quét");
                list1.Add("Truy lãnh tháng trước");


                var list2 = new List<string>();
                list2.Add("STT");
                list2.Add("Mã nhân viên");
                list2.Add("Họ tên");
                list2.Add("Công ty");
                list2.Add("Chức vụ");
                list2.Add("Phòng ban");
                list2.Add("Bậc");
                list2.Add("Mức");
                list2.Add("Tổng lương");
                list2.Add("Lương cơ bản");
                list2.Add("Lương thành tích");
                list2.Add("Lương kiêm nhiệm");
                list2.Add("Lương đóng BHXH");
                list2.Add("Ngày công chuẩn");
                list2.Add("Ngày công tính lương");
                list2.Add("Phép (P)");
                list2.Add("Nghỉ bù (NB)");
                list2.Add("Lễ (L)");
                list2.Add("Hưởng lương (R)");
                list2.Add("Không lương (RO)");
                list2.Add("Ngày thường");
                list2.Add("Ngày CN");
                list2.Add("Ngày lễ");
                list2.Add("Lương CB theo ngày công thực tế");
                list2.Add("Tiền ăn giữa ca");
                list2.Add("Đi lại");
                list2.Add("Điện thoại");
                list2.Add("Xăng xe");
                list2.Add("Thu hút");
                list2.Add("Thâm niên");

                //list2.Add("Sinh hoạt phí");
                list2.Add("Phụ cấp khác (bao gồm p/c chức danh)");
                list2.Add("Công tác phí");

                list2.Add("Truy lãnh");
                list2.Add("Thu nhập khác");
                list2.Add("Tiền cơm công tác");
                list2.Add("Tổng thu nhập");
                list2.Add("Tổng thu nhập chịu thuế");
                list2.Add("Số người phụ thuộc");
                list2.Add("Gia cảnh");
                list2.Add("Bản thân");
                list2.Add("Thu nhập tính thuế TNCN");
                list2.Add("BHXH (8%)");
                list2.Add("BHYT (1.5%)");
                list2.Add("BHTN (1%)");
                list2.Add("Quỹ nội bộ (1%)");
                list2.Add("Thuế TNCN (Tạm thu)");
                list2.Add("Truy thu");

                list2.Add("Truy thu bảo hiểm");
                list2.Add("Tổng thu nhập thực tế");

                list2.Add("Tạm ứng");
                //list2.Add("Quyết toán lương tháng 1");
                list2.Add("Truy thu thuế TNCN 2017");
                list2.Add("Truy lãnh thuế TNCN 2017");
                list2.Add("THỰC LÃNH");
                list2.Add("BHXH (17.5%)");
                list2.Add("BHYT (3%)");
                list2.Add("BHTN (1%)");
                list2.Add("Ngày quét");
                list2.Add("Truy lãnh tháng trước");
                list2.Add("Tên ngân hàng");
                list2.Add("Số tài khoản");
                list2.Add("CMND");
                list2.Add("Họ tên không dấu");
                var list3 = new List<string>();
                for (var col = 1; col <= 55; col++)
                {
                    list3.Add("" + col + "");
                }

                var idRowStart = 2; // bat dau o dong thu 4
                var headerRow = sheet.CreateRow(idRowStart);
                int rowend = idRowStart;
                ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);
                idRowStart++;
                var headerRow1 = sheet.CreateRow(idRowStart);
                ReportHelperExcel.CreateHeaderRow(headerRow1, 0, styleheadedColumnTable, list2);

                idRowStart++;
                var headerRow2 = sheet.CreateRow(idRowStart);
                ReportHelperExcel.CreateHeaderRow(headerRow2, 0, styleheadedColumnTable, list3);
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 53, 55));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 52, 52));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 51, 51));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 50, 50));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 49, 49));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 48, 48));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 47, 47));

                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 46, 46));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 45, 45));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 44, 44));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 41, 43));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 40, 40));

                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 38, 39));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 33, 33));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 37, 37));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 36, 36));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 35, 35));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 34, 34));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 33, 33));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 32, 32));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 31, 31));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 30, 30));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 25, 30));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 24, 29));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 23, 23));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 20, 22));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 15, 19));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 14, 14));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 13, 13));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 12, 12));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 11, 11));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 10, 10));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 9, 9));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 8, 8));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 7, 7));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 6, 6));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 5, 5));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 4, 4));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 3, 3));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 2, 2));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 1, 1));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 0, 0));



                sheet.SetColumnWidth(0, 8 * 210);
                sheet.SetColumnWidth(1, 30 * 210);
                sheet.SetColumnWidth(2, 30 * 310);
                sheet.SetColumnWidth(3, 30 * 210);
                sheet.SetColumnWidth(4, 8 * 210);
                sheet.SetColumnWidth(5, 15 * 210);
                sheet.SetColumnWidth(6, 15 * 210);
                sheet.SetColumnWidth(7, 15 * 210);
                sheet.SetColumnWidth(8, 15 * 210);
                sheet.SetColumnWidth(9, 15 * 210);
                sheet.SetColumnWidth(10, 15 * 210);
                sheet.SetColumnWidth(11, 15 * 210);

                sheet.SetColumnWidth(12, 15 * 210);
                sheet.SetColumnWidth(13, 15 * 210);
                sheet.SetColumnWidth(14, 15 * 210);
                sheet.SetColumnWidth(15, 10 * 210);
                sheet.SetColumnWidth(16, 15 * 210);
                sheet.SetColumnWidth(17, 15 * 210);
                sheet.SetColumnWidth(18, 15 * 210);
                sheet.SetColumnWidth(19, 15 * 210);
                sheet.SetColumnWidth(20, 20 * 210);
                sheet.SetColumnWidth(21, 15 * 210);
                sheet.SetColumnWidth(22, 15 * 210);
                sheet.SetColumnWidth(23, 15 * 210);

                sheet.SetColumnWidth(24, 15 * 210);
                sheet.SetColumnWidth(25, 15 * 210);
                sheet.SetColumnWidth(26, 15 * 210);
                sheet.SetColumnWidth(27, 15 * 210);
                sheet.SetColumnWidth(28, 15 * 210);
                sheet.SetColumnWidth(29, 15 * 210);
                sheet.SetColumnWidth(30, 15 * 210);
                sheet.SetColumnWidth(31, 15 * 210);
                sheet.SetColumnWidth(32, 15 * 210);
                sheet.SetColumnWidth(33, 15 * 210);
                sheet.SetColumnWidth(34, 15 * 210);
                sheet.SetColumnWidth(35, 15 * 210);

                sheet.SetColumnWidth(36, 15 * 210);
                sheet.SetColumnWidth(37, 15 * 210);
                sheet.SetColumnWidth(38, 15 * 210);
                sheet.SetColumnWidth(39, 15 * 210);
                sheet.SetColumnWidth(40, 15 * 210);
                sheet.SetColumnWidth(41, 15 * 210);
                sheet.SetColumnWidth(42, 15 * 210);
                sheet.SetColumnWidth(43, 15 * 210);
                sheet.SetColumnWidth(44, 15 * 210);
                sheet.SetColumnWidth(45, 15 * 210);
                sheet.SetColumnWidth(46, 15 * 210);
                sheet.SetColumnWidth(47, 15 * 210);
                sheet.SetColumnWidth(48, 15 * 210);
                sheet.SetColumnWidth(49, 15 * 210);
                sheet.SetColumnWidth(50, 15 * 210);
                sheet.SetColumnWidth(51, 20 * 400);
                sheet.SetColumnWidth(52, 20 * 400);
                sheet.SetColumnWidth(53, 20 * 400);
                sheet.SetColumnWidth(54, 20 * 400);
                sheet.SetColumnWidth(55, 20 * 400);
                sheet.SetColumnWidth(56, 20 * 400);
                sheet.SetColumnWidth(57, 20 * 400);
                sheet.SetColumnWidth(58, 20 * 400);
                sheet.SetColumnWidth(59, 20 * 400);
                sheet.SetColumnWidth(60, 20 * 400);
                sheet.SetColumnWidth(61, 20 * 400);
                sheet.SetColumnWidth(62, 20 * 400);

                var data = nhanSuContext.sp_NS_BangLuongNhanVien(maPhongBan, thang, nam, qSearch).ToList();
                var lstPhongBan = nhanSuContext.sp_NS_BangLuongNhanVien(maPhongBan, thang, nam, qSearch).Select(d => d.phongBan).Distinct().ToList();
                var stt = 0;
                var sttPB = 0;
                int dem = 0;
                decimal tongThuNhapSum = 0;
                decimal tongThucLanhSum = 0;
                decimal SumtongThuNhapChiuThue = 0;
                foreach (var itemPB in lstPhongBan)
                {
                    decimal tongSo = data.Where(d => d.phongBan == itemPB).Count();
                    dem = 0;
                    stt = 0;
                    idRowStart++;
                    rowC = sheet.CreateRow(idRowStart);
                    ReportHelperExcel.SetAlignment(rowC, dem++, (++sttPB).ToString(), styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", tongSo) + " nhân viên", styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, itemPB, styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, "", styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, "", styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, "", styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, "", styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, "", styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.tongLuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapLuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.khoanBoSungLuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongChoViec)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongDongBaoHiem)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.ngayCongChuan)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.tongNgayCong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayNghiPhep)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayNghiBu)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayNghiLe)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayNghiKhacDuocHuongLuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayNghiKhongLuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayTangCaThuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayTangCaChuNhat)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayTangCaNgayLe)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThang)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapTienAn)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDiLaiNew ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDienThoai)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", 0), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapThuHut)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapThamNien)), styleCellSumary);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.sinhHoatPhi)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.congTacPhi)), styleCellSumary);


                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.truyLanh)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapKhac ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThanhTich1 ?? 0)), styleCellSumary);
                    //tổng thu nhập
                    var tongThuNhap =
                        (data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThang ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapTienAn ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDienThoai ?? 0))
                        + (Convert.ToDouble(data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDiLaiNew ?? 0)))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapThuHut ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapThamNien ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.congTacPhi ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.sinhHoatPhi ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.truyLanh ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapKhac ?? 0)) + (data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThanhTich1 ?? 0));

                    tongThuNhapSum = tongThuNhapSum + Convert.ToDecimal(tongThuNhap);
                    //TongThucLanh
                    var LuongTheoNgayCong = (data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThang ?? 0));
                    var TongTienPhuCap = (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapTienAn ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDienThoai ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapThuHut ?? 0)) + (Convert.ToDouble(data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDiLaiNew ?? 0)));

                    //var TongThuNhap = LuongTheoNgayCong + TongTienPhuCap + (item.congTacPhi ?? 0) + (item.sinhHoatPhi ?? 0);

                    var TongKhauTru = (data.Where(d => d.phongBan == itemPB).Sum(d => d.baoHiemXH ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.baoHiemYTe ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.baoHiemTN ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.thue ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.doanPhi ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.truyThu ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.dangPhi ?? 0));
                    foreach (var item in data.Where(d => d.phongBan == itemPB))
                    {
                        if (item.phuCapDiLai == 2)
                        {
                            var TruBaoHiem = (item.baoHiemXH ?? 0) + (item.baoHiemYTe ?? 0) + (item.baoHiemTN ?? 0);
                            TongKhauTru = TongKhauTru - TruBaoHiem;
                        }
                        if (item.phuCapTienXang == 2)
                        {
                            TongKhauTru = TongKhauTru - (item.thue ?? 0);
                        }
                    }
                    //var TongPhuCapTL = (item.truyLanh ?? 0) - (item.truyThu ?? 0) - (item.dangPhi ?? 0) ;
                    var TongThucLanh = tongThuNhap - TongKhauTru; //+ TongPhuCapTL;



                    var tongThuNhapChiuThue = tongThuNhap - (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapTienAn ?? 0)) - (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDienThoai ?? 0))
                        - (data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThanhTich1 ?? 0));
                    SumtongThuNhapChiuThue = SumtongThuNhapChiuThue + Convert.ToDecimal(tongThuNhapChiuThue);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", tongThuNhap), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", tongThuNhapChiuThue), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNguoiPhuThuoc)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.giamTruNguoiPhuThuoc)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.giamTruBanThan)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.tongThuNhapChiuThue)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.baoHiemXH ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.baoHiemYTe ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.baoHiemTN ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.doanPhi ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.thue)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.truyThu)), styleCellSumary);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.dangPhi)), styleCellSumary);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", TongThucLanh), styleCellSumary);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.tamUng ?? 0)), styleCellSumary);

                    //ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongThanhTich2 ?? 0), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThanhTich3 ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayPhepTang)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.thucLanh ?? 0)), styleCellSumary);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongDongBaoHiem ?? 0) * 17.5 / 100), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongDongBaoHiem ?? 0) * 3 / 100), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongDongBaoHiem ?? 0) * 1 / 100), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayQuet)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.truyLanhThangTruoc ?? 0)), styleCellSumary);
                    // nhan vien
                    foreach (var item in data.Where(d => d.phongBan == itemPB))
                    {
                        dem = 0;

                        idRowStart++;
                        rowC = sheet.CreateRow(idRowStart);
                        ReportHelperExcel.SetAlignment(rowC, dem++, sttPB + "." + (++stt).ToString(), hStyleConCenter);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.maNhanVien, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.hoTen, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.tenKhoiTinhLuong, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.chucVu, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.phongBan, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.bac), hStyleConCenter);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.hinhThucLuong, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.tongLuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapLuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.khoanBoSungLuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongChoViec), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongDongBaoHiem), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.ngayCongChuan), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.tongNgayCong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayNghiPhep), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayNghiBu), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayNghiLe), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayNghiKhacDuocHuongLuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayNghiKhongLuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayTangCaThuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayTangCaChuNhat), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayTangCaNgayLe), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongThang), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapTienAn), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapDiLaiNew ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapDienThoai), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapThuHut), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapThamNien), hStyleConRight);

                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.sinhHoatPhi), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.congTacPhi), hStyleConRight);


                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.truyLanh), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapKhac ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongThanhTich1 ?? 0), hStyleConRight);
                        //tổng thu nhập
                        tongThuNhap = (item.luongThang ?? 0) + (item.phuCapTienAn ?? 0) + (item.phuCapDienThoai ?? 0)
                            + (Convert.ToDouble(item.phuCapDiLaiNew ?? 0))
                           + (item.phuCapThuHut ?? 0) + (item.phuCapThamNien ?? 0) + (item.congTacPhi ?? 0) + (item.sinhHoatPhi ?? 0) + (item.truyLanh ?? 0)
                           + (item.phuCapKhac ?? 0) + (item.luongThanhTich1 ?? 0);

                        tongThuNhapSum = tongThuNhapSum + Convert.ToDecimal(tongThuNhap);


                        //TongThucLanh
                        LuongTheoNgayCong = (item.luongThang ?? 0);
                        TongTienPhuCap = (item.phuCapTienAn ?? 0) + (item.phuCapDienThoai ?? 0) + (item.phuCapThuHut ?? 0) + (Convert.ToDouble(item.phuCapDiLaiNew ?? 0));

                        //var TongThuNhap = LuongTheoNgayCong + TongTienPhuCap + (item.congTacPhi ?? 0) + (item.sinhHoatPhi ?? 0);

                        TongKhauTru = (item.baoHiemXH ?? 0) + (item.baoHiemYTe ?? 0) + (item.baoHiemTN ?? 0) + (item.thue ?? 0) + (item.doanPhi ?? 0) + (item.truyThu ?? 0) + (item.dangPhi ?? 0);
                        if (item.phuCapDiLai == 2)
                        {
                            var TruBaoHiem = (item.baoHiemXH ?? 0) + (item.baoHiemYTe ?? 0) + (item.baoHiemTN ?? 0);
                            TongKhauTru = TongKhauTru - TruBaoHiem;
                        }
                        if (item.phuCapTienXang == 2)
                        {
                            TongKhauTru = TongKhauTru - (item.thue ?? 0);
                        }
                        //var TongPhuCapTL = (item.truyLanh ?? 0) - (item.truyThu ?? 0) - (item.dangPhi ?? 0) ;
                        TongThucLanh = tongThuNhap - TongKhauTru; //+ TongPhuCapTL;
                        tongThucLanhSum = tongThucLanhSum + Convert.ToDecimal(TongThucLanh);
                        tongThuNhapChiuThue = tongThuNhap - (item.phuCapTienAn ?? 0) - (item.phuCapDienThoai ?? 0) - (item.luongThanhTich1 ?? 0);
                        SumtongThuNhapChiuThue = SumtongThuNhapChiuThue + Convert.ToDecimal(tongThuNhapChiuThue);

                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", tongThuNhap), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", tongThuNhapChiuThue), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.soNguoiPhuThuoc), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.giamTruNguoiPhuThuoc), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.giamTruBanThan), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.tongThuNhapChiuThue), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.baoHiemXH ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.baoHiemYTe ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.baoHiemTN ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.doanPhi ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.thue), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.truyThu), hStyleConRight);

                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.dangPhi), hStyleConRight);

                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", TongThucLanh), hStyleConRight);

                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.tamUng ?? 0), hStyleConRight);

                        //ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongThanhTich2 ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongThanhTich3 ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.soNgayPhepTang), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.thucLanh ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongDongBaoHiem * 17.5 / 100), styleCellSumary);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongDongBaoHiem * 3 / 100), styleCellSumary);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongDongBaoHiem * 1 / 100), styleCellSumary);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayQuet), styleCellSumary);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.truyLanhThangTruoc ?? 0), styleCellSumary);

                        ReportHelperExcel.SetAlignment(rowC, dem++, item.ghiChu, styleCellSumary);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.soTaiKhoan, styleCellSumary);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.soCMND, styleCellSumary);
                        ReportHelperExcel.SetAlignment(rowC, dem++, CharacterHelper.RemoveUnicode(item.hoTen), styleCellSumary);

                    }
                }


                #region dòng tổng cộng
                idRowStart++;
                int demT = 0;
                int tongSoNV = data.Count();
                Row rowT = sheet.CreateRow(idRowStart);
                CellRangeAddress cellRangeAddressT = new CellRangeAddress(idRowStart, idRowStart, demT, demT + 7);
                sheet.AddMergedRegion(cellRangeAddressT);
                ReportHelperExcel.SetAlignment(rowT, demT++, "Tổng cộng: " + String.Format("{0:#,##0}", tongSoNV) + " nhân viên", styleheadedColumnTable);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", styleheadedColumnTable);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConCenter);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConCenter);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.tongLuong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapLuong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.khoanBoSungLuong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongChoViec)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongDongBaoHiem)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.ngayCongChuan)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.tongNgayCong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayNghiPhep)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayNghiBu)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayNghiLe)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayNghiKhacDuocHuongLuong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayNghiKhongLuong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayTangCaThuong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayTangCaChuNhat)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayTangCaNgayLe)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongThang)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapTienAn)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapDiLaiNew)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapDienThoai)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", 0), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapThuHut)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapThamNien)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.sinhHoatPhi)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.congTacPhi)), styleCellSumary);


                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.truyLanh)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapKhac)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongThanhTich1)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", (tongThuNhapSum / 2)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", SumtongThuNhapChiuThue), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.soNguoiPhuThuoc)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.giamTruNguoiPhuThuoc)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.giamTruBanThan)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.tongThuNhapChiuThue)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.baoHiemXH)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.baoHiemYTe)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.baoHiemTN)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.doanPhi)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.thue)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.truyThu)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.dangPhi)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", tongThucLanhSum), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.tamUng)), styleCellSumary);
                //ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongThanhTich2)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongThanhTich3)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.soNgayPhepTang)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.thucLanh)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongDongBaoHiem) * 17.5 / 100), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongDongBaoHiem) * 3 / 100), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongDongBaoHiem) * 1 / 100), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayQuet)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.truyLanhThangTruoc ?? 0)), styleCellSumary);

                #endregion

                //idRowStart = idRowStart + 2;
                //string cellFooterGD = "GIÁM ĐỐC NHÂN SỰ-QTVP";
                //var titleCellFooterGD = HSSFCellUtil.CreateCell(sheet.CreateRow(idRowStart), 4, cellFooterGD.ToUpper());
                //titleCellFooterGD.CellStyle = styleTitle;

                //string cellFooterKT = "PHÒNG TÀI CHÍNH - KẾ TOÁN";
                //var titleCellFooterKT = HSSFCellUtil.CreateCell(sheet.GetRow(idRowStart), 21, cellFooterKT.ToUpper());
                //titleCellFooterKT.CellStyle = styleTitle;

                //string cellFooterTGD = "TỔNG GIÁM ĐỐC";
                //var titleCellFooterTGD = HSSFCellUtil.CreateCell(sheet.GetRow(idRowStart), 42, cellFooterTGD.ToUpper());
                //titleCellFooterTGD.CellStyle = styleTitle;

                var stream = new MemoryStream();
                workbook.Write(stream);

                Response.ContentType = "application/vnd.ms-excel";
                Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", filename));
                Response.Clear();

                Response.BinaryWrite(stream.GetBuffer());
                Response.End();
            }
            catch
            {

            }

        }
        public void XuatFileBLNVWindow(int thang, int nam, string maPhongBan, string qSearch)
        {

            try
            {
                #region Role user
                permission = GetPermission(MCV, BangPhanQuyen.QuyenIn);
                if (!permission.HasValue)
                    return;
                if (!permission.Value)
                    return;
                #endregion

                var filename = "";
                var virtualPath = HttpRuntime.AppDomainAppVirtualPath;

                var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplate.xls"), FileMode.Open, FileAccess.Read);

                var workbook = new HSSFWorkbook(fileStream, true);
                filename += "BangLuongNV_" + nam + "_" + thang + ".xls";


                var sheet = workbook.GetSheet("danhsachnhanvien");


                #region format style excel
                /*style title start*/
                //tạo font cho các title
                //font tiêu đề 
                HSSFFont hFontTieuDe = (HSSFFont)workbook.CreateFont();
                hFontTieuDe.FontHeightInPoints = 18;
                hFontTieuDe.Boldweight = 100 * 10;
                hFontTieuDe.FontName = "Times New Roman";
                hFontTieuDe.Color = HSSFColor.BLUE.index;

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
                //font chứ hoa đậm
                HSSFFont hFontNommalUpperRED = (HSSFFont)workbook.CreateFont();
                hFontNommalUpperRED.Boldweight = (short)FontBoldWeight.BOLD;
                hFontNommalUpperRED.Color = HSSFColor.RED.index;
                hFontNommalUpperRED.FontName = "Times New Roman";

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

                //style sum cell
                var styleCellSumary = workbook.CreateCellStyle();
                styleCellSumary.SetFont(hFontNommalUpper);
                styleCellSumary.WrapText = true;
                styleCellSumary.BorderBottom = CellBorderType.THIN;
                styleCellSumary.BorderLeft = CellBorderType.THIN;
                styleCellSumary.BorderRight = CellBorderType.THIN;
                styleCellSumary.BorderTop = CellBorderType.THIN;
                styleCellSumary.VerticalAlignment = VerticalAlignment.CENTER;
                styleCellSumary.Alignment = HorizontalAlignment.RIGHT;
                //style sum cell
                var styleCellSumaryTen = workbook.CreateCellStyle();
                styleCellSumaryTen.SetFont(hFontNommalUpper);
                styleCellSumaryTen.WrapText = true;
                styleCellSumaryTen.BorderBottom = CellBorderType.THIN;
                styleCellSumaryTen.BorderLeft = CellBorderType.THIN;
                styleCellSumaryTen.BorderRight = CellBorderType.THIN;
                styleCellSumaryTen.BorderTop = CellBorderType.THIN;
                styleCellSumaryTen.VerticalAlignment = VerticalAlignment.CENTER;
                styleCellSumaryTen.Alignment = HorizontalAlignment.LEFT;

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

                string cellTenCty = "CÔNG TY CỔ PHẦN TV.WINDOW";
                var titleCellCty = HSSFCellUtil.CreateCell(sheet.CreateRow(0), 0, cellTenCty.ToUpper());
                titleCellCty.CellStyle = styleTitle;

                //string cellEnd1 = "Bảng Lương Tháng " + thang + " năm " + nam;
                //var titleCellEnd1 = HSSFCellUtil.CreateCell(sheet.GetRow(0), 43, cellEnd1.ToUpper());
                //titleCellEnd1.CellStyle = styleTitle;

                string rowtitle = "Bảng Lương Công Ty Tháng " + thang + " năm " + nam;
                var titleCell = HSSFCellUtil.CreateCell(sheet.CreateRow(firstRowNumber), 15, rowtitle.ToUpper());
                titleCell.CellStyle = styleTitle;

                //string cellEnd2 = "";
                //var titleCellEnd2 = HSSFCellUtil.CreateCell(sheet.GetRow(1), 43, cellEnd2.ToUpper());
                //titleCellEnd2.CellStyle = styleTitle;

                ++firstRowNumber;

                var list1 = new List<string>();
                list1.Add("STT");
                list1.Add("Mã nhân viên");
                list1.Add("Họ tên");
                list1.Add("Công ty");
                list1.Add("Chức vụ");
                list1.Add("Phòng ban");
                list1.Add("Bậc");
                list1.Add("Mức");
                list1.Add("Tổng lương");
                list1.Add("Lương cơ bản");
                list1.Add("Lương thành tích");
                list1.Add("Lương kiêm nhiệm");
                list1.Add("Lương đóng BHXH");
                list1.Add("Ngày công chuẩn");
                list1.Add("Ngày công tính lương");
                list1.Add("Ngày nghỉ");// 
                list1.Add("");//
                list1.Add("");//
                list1.Add("");//
                list1.Add("");//
                list1.Add("Tăng ca");//
                list1.Add("");//
                list1.Add("");   //         
                list1.Add("Lương CB theo ngày công thực tế");
                list1.Add("Các khoản phụ cấp");
                list1.Add("");//
                list1.Add("");//
                list1.Add("");//
                list1.Add("");//
                list1.Add("");//

                list1.Add("Phụ cấp khác");
                list1.Add("Công tác phí");

                list1.Add("Truy lãnh");
                list1.Add("Thu nhập khác");
                list1.Add("Tiền cơm công tác");
                list1.Add("Tổng thu nhập");

                list1.Add("Tổng thu nhập chịu thuế");

                list1.Add("Số người phụ thuộc");
                list1.Add("Các khoản giảm trừ không tính thuế TNCN");
                list1.Add("");
                list1.Add("Thu nhập tính thuế TNCN");
                list1.Add("Các khoản trích trừ vào lương");
                list1.Add("");
                list1.Add("");
                list1.Add("Quỹ nội bộ (1%)");
                list1.Add("Thuế TNCN (Tạm thu)");
                list1.Add("Truy thu");

                list1.Add("Truy thu bảo hiểm");
                list1.Add("Tổng thu nhập thực tế");

                list1.Add("Tạm ứng");
                //list1.Add("Quyết toán lương tháng 1");
                list1.Add("Truy thu thuế TNCN 2017");
                list1.Add("Truy lãnh thuế TNCN 2017");
                list1.Add("THỰC LÃNH");
                list1.Add("DN đóng BHXH");
                list1.Add("");
                list1.Add("Số ngày quét");
                list1.Add("Truy lãnh tháng trước");

                var list2 = new List<string>();
                list2.Add("STT");
                list2.Add("Mã nhân viên");
                list2.Add("Họ tên");
                list2.Add("Công ty");
                list2.Add("Chức vụ");
                list2.Add("Phòng ban");
                list2.Add("Bậc");
                list2.Add("Mức");
                list2.Add("Tổng lương");
                list2.Add("Lương cơ bản");
                list2.Add("Lương thành tích");
                list2.Add("Lương kiêm nhiệm");
                list2.Add("Lương đóng BHXH");
                list2.Add("Ngày công chuẩn");
                list2.Add("Ngày công tính lương");
                list2.Add("Phép (P)");
                list2.Add("Nghỉ bù (NB)");
                list2.Add("Lễ (L)");
                list2.Add("Hưởng lương (R)");
                list2.Add("Không lương (RO)");
                list2.Add("Ngày thường");
                list2.Add("Ngày CN");
                list2.Add("Ngày lễ");
                list2.Add("Lương CB theo ngày công thực tế");
                list2.Add("Tiền ăn giữa ca");
                list2.Add("Đi lại");
                list2.Add("Điện thoại");
                list2.Add("Xăng xe");
                list2.Add("Thu hút");
                list2.Add("Thâm niên");

                //list2.Add("Sinh hoạt phí");
                list2.Add("Phụ cấp khác (bao gồm p/c chức danh)");
                list2.Add("Công tác phí");

                list2.Add("Truy lãnh");
                list2.Add("Thu nhập khác");
                list2.Add("Tiền cơm công tác");
                list2.Add("Tổng thu nhập");
                list2.Add("Tổng thu nhập chịu thuế");
                list2.Add("Số người phụ thuộc");
                list2.Add("Gia cảnh");
                list2.Add("Bản thân");
                list2.Add("Thu nhập tính thuế TNCN");
                list2.Add("BHXH (8%)");
                list2.Add("BHYT (1.5%)");
                list2.Add("BHTN (1%)");
                list2.Add("Quỹ nội bộ (1%)");
                list2.Add("Thuế TNCN (Tạm thu)");
                list2.Add("Truy thu");

                list2.Add("Truy thu bảo hiểm");
                list2.Add("Tổng thu nhập thực tế");

                list2.Add("Tạm ứng");
                //list2.Add("Quyết toán lương tháng 1");
                list2.Add("Truy thu thuế TNCN 2017");
                list2.Add("Truy lãnh thuế TNCN 2017");
                list2.Add("THỰC LÃNH");
                list2.Add("BHXH (17.5%)");
                list2.Add("BHYT (3%)");
                list2.Add("BHTN (1%)");

                list2.Add("Số ngày quét");
                list2.Add("Truy lãnh tháng trước");
                var list3 = new List<string>();
                for (var col = 1; col <= 55; col++)
                {
                    list3.Add("" + col + "");
                }

                var idRowStart = 2; // bat dau o dong thu 4
                var headerRow = sheet.CreateRow(idRowStart);
                int rowend = idRowStart;
                ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);
                idRowStart++;
                var headerRow1 = sheet.CreateRow(idRowStart);
                ReportHelperExcel.CreateHeaderRow(headerRow1, 0, styleheadedColumnTable, list2);

                idRowStart++;
                var headerRow2 = sheet.CreateRow(idRowStart);
                ReportHelperExcel.CreateHeaderRow(headerRow2, 0, styleheadedColumnTable, list3);
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 53, 55));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 52, 52));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 51, 51));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 50, 50));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 49, 49));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 48, 48));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 47, 47));

                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 46, 46));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 45, 45));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 44, 44));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 41, 43));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 40, 40));

                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 38, 39));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 33, 33));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 37, 37));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 36, 36));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 35, 35));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 34, 34));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 33, 33));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 32, 32));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 31, 31));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 30, 30));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 25, 30));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 24, 29));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 23, 23));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 20, 22));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 15, 19));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 14, 14));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 13, 13));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 12, 12));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 11, 11));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 10, 10));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 9, 9));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 8, 8));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 7, 7));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 6, 6));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 5, 5));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 4, 4));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 3, 3));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 2, 2));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 1, 1));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 0, 0));



                sheet.SetColumnWidth(0, 8 * 210);
                sheet.SetColumnWidth(1, 30 * 210);
                sheet.SetColumnWidth(2, 30 * 310);
                sheet.SetColumnWidth(3, 30 * 210);
                sheet.SetColumnWidth(4, 8 * 210);
                sheet.SetColumnWidth(5, 15 * 210);
                sheet.SetColumnWidth(6, 15 * 210);
                sheet.SetColumnWidth(7, 15 * 210);
                sheet.SetColumnWidth(8, 15 * 210);
                sheet.SetColumnWidth(9, 15 * 210);
                sheet.SetColumnWidth(10, 15 * 210);
                sheet.SetColumnWidth(11, 15 * 210);

                sheet.SetColumnWidth(12, 15 * 210);
                sheet.SetColumnWidth(13, 15 * 210);
                sheet.SetColumnWidth(14, 15 * 210);
                sheet.SetColumnWidth(15, 10 * 210);
                sheet.SetColumnWidth(16, 15 * 210);
                sheet.SetColumnWidth(17, 15 * 210);
                sheet.SetColumnWidth(18, 15 * 210);
                sheet.SetColumnWidth(19, 15 * 210);
                sheet.SetColumnWidth(20, 20 * 210);
                sheet.SetColumnWidth(21, 15 * 210);
                sheet.SetColumnWidth(22, 15 * 210);
                sheet.SetColumnWidth(23, 15 * 210);

                sheet.SetColumnWidth(24, 15 * 210);
                sheet.SetColumnWidth(25, 15 * 210);
                sheet.SetColumnWidth(26, 15 * 210);
                sheet.SetColumnWidth(27, 15 * 210);
                sheet.SetColumnWidth(28, 15 * 210);
                sheet.SetColumnWidth(29, 15 * 210);
                sheet.SetColumnWidth(30, 15 * 210);
                sheet.SetColumnWidth(31, 15 * 210);
                sheet.SetColumnWidth(32, 15 * 210);
                sheet.SetColumnWidth(33, 15 * 210);
                sheet.SetColumnWidth(34, 15 * 210);
                sheet.SetColumnWidth(35, 15 * 210);

                sheet.SetColumnWidth(36, 15 * 210);
                sheet.SetColumnWidth(37, 15 * 210);
                sheet.SetColumnWidth(38, 15 * 210);
                sheet.SetColumnWidth(39, 15 * 210);
                sheet.SetColumnWidth(40, 15 * 210);
                sheet.SetColumnWidth(41, 15 * 210);
                sheet.SetColumnWidth(42, 15 * 210);
                sheet.SetColumnWidth(43, 15 * 210);
                sheet.SetColumnWidth(44, 15 * 210);
                sheet.SetColumnWidth(45, 15 * 210);
                sheet.SetColumnWidth(46, 15 * 210);
                sheet.SetColumnWidth(47, 15 * 210);
                sheet.SetColumnWidth(48, 15 * 210);
                sheet.SetColumnWidth(49, 15 * 210);
                sheet.SetColumnWidth(50, 15 * 210);
                sheet.SetColumnWidth(51, 20 * 400);
                sheet.SetColumnWidth(52, 20 * 400);
                sheet.SetColumnWidth(53, 20 * 400);
                sheet.SetColumnWidth(54, 20 * 400);
                sheet.SetColumnWidth(55, 20 * 400);
                sheet.SetColumnWidth(56, 20 * 400);
                sheet.SetColumnWidth(57, 20 * 400);
                var data = nhanSuContext.sp_NS_BangLuongNhanVien_TVWINDOW(maPhongBan, thang, nam, qSearch).ToList();

                var lstPhongBan = nhanSuContext.sp_NS_BangLuongNhanVien_TVWINDOW(maPhongBan, thang, nam, qSearch).Select(d => d.phongBan).Distinct().ToList();
                var stt = 0;
                var sttPB = 0;
                int dem = 0;
                decimal tongThuNhapSum = 0;
                decimal tongThucLanhSum = 0;
                decimal SumtongThuNhapChiuThue = 0;
                foreach (var itemPB in lstPhongBan)
                {
                    decimal tongSo = data.Where(d => d.phongBan == itemPB).Count();

                    dem = 0;
                    stt = 0;
                    idRowStart++;
                    rowC = sheet.CreateRow(idRowStart);
                    ReportHelperExcel.SetAlignment(rowC, dem++, (++sttPB).ToString(), styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", tongSo) + " nhân viên", styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, itemPB, styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, "", styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, "", styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, "", styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, "", styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, "", styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.tongLuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapLuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.khoanBoSungLuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongChoViec)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongDongBaoHiem)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.ngayCongChuan)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.tongNgayCong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayNghiPhep)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayNghiBu)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayNghiLe)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayNghiKhacDuocHuongLuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayNghiKhongLuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayTangCaThuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayTangCaChuNhat)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayTangCaNgayLe)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThang)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapTienAn)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDiLaiNew ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDienThoai)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", 0), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapThuHut)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapThamNien)), styleCellSumary);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.sinhHoatPhi)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.congTacPhi)), styleCellSumary);


                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.truyLanh)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapKhac ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThanhTich1 ?? 0)), styleCellSumary);
                    //tổng thu nhập
                    var tongThuNhap =
                        (data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThang ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapTienAn ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDienThoai ?? 0))
                        + (Convert.ToDouble(data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDiLaiNew ?? 0)))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapThuHut ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapThamNien ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.congTacPhi ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.sinhHoatPhi ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.truyLanh ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapKhac ?? 0)) + (data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThanhTich1 ?? 0));

                    tongThuNhapSum = tongThuNhapSum + Convert.ToDecimal(tongThuNhap);
                    //TongThucLanh
                    var LuongTheoNgayCong = (data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThang ?? 0));
                    var TongTienPhuCap = (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapTienAn ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDienThoai ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapThuHut ?? 0)) + (Convert.ToDouble(data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDiLaiNew ?? 0)));

                    //var TongThuNhap = LuongTheoNgayCong + TongTienPhuCap + (item.congTacPhi ?? 0) + (item.sinhHoatPhi ?? 0);

                    var TongKhauTru = (data.Where(d => d.phongBan == itemPB).Sum(d => d.baoHiemXH ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.baoHiemYTe ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.baoHiemTN ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.thue ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.doanPhi ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.truyThu ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.dangPhi ?? 0));
                    foreach (var item in data.Where(d => d.phongBan == itemPB))
                    {
                        if (item.phuCapDiLai == 2)
                        {
                            var TruBaoHiem = (item.baoHiemXH ?? 0) + (item.baoHiemYTe ?? 0) + (item.baoHiemTN ?? 0);
                            TongKhauTru = TongKhauTru - TruBaoHiem;
                        }
                        if (item.phuCapTienXang == 2)
                        {
                            TongKhauTru = TongKhauTru - (item.thue ?? 0);
                        }
                    }
                    //var TongPhuCapTL = (item.truyLanh ?? 0) - (item.truyThu ?? 0) - (item.dangPhi ?? 0) ;
                    var TongThucLanh = tongThuNhap - TongKhauTru; //+ TongPhuCapTL;



                    var tongThuNhapChiuThue = tongThuNhap - (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapTienAn ?? 0)) - (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDienThoai ?? 0))
                        - (data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThanhTich1 ?? 0));
                    SumtongThuNhapChiuThue = SumtongThuNhapChiuThue + Convert.ToDecimal(tongThuNhapChiuThue);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", tongThuNhap), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", tongThuNhapChiuThue), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNguoiPhuThuoc)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.giamTruNguoiPhuThuoc)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.giamTruBanThan)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.tongThuNhapChiuThue)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.baoHiemXH ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.baoHiemYTe ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.baoHiemTN ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.doanPhi ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.thue)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.truyThu)), styleCellSumary);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.dangPhi)), styleCellSumary);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", TongThucLanh), styleCellSumary);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.tamUng ?? 0)), styleCellSumary);

                    //ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongThanhTich2 ?? 0), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThanhTich3 ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayPhepTang)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.thucLanh ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongDongBaoHiem ?? 0) * 17.5 / 100), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongDongBaoHiem ?? 0) * 3 / 100), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongDongBaoHiem ?? 0) * 1 / 100), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayQuet)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.truyLanhThangTruoc ?? 0)), styleCellSumary);

                    // nhan vien
                    foreach (var item in data.Where(d => d.phongBan == itemPB))
                    {
                        dem = 0;

                        idRowStart++;
                        rowC = sheet.CreateRow(idRowStart);
                        ReportHelperExcel.SetAlignment(rowC, dem++, sttPB + "." + (++stt).ToString(), hStyleConCenter);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.maNhanVien, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.hoTen, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.tenKhoiTinhLuong, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.chucVu, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.phongBan, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.bac), hStyleConCenter);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.hinhThucLuong, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.tongLuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapLuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.khoanBoSungLuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongChoViec), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongDongBaoHiem), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.ngayCongChuan), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.tongNgayCong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayNghiPhep), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayNghiBu), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayNghiLe), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayNghiKhacDuocHuongLuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayNghiKhongLuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayTangCaThuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayTangCaChuNhat), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayTangCaNgayLe), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongThang), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapTienAn), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapDiLaiNew ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapDienThoai), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapThuHut), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapThamNien), hStyleConRight);

                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.sinhHoatPhi), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.congTacPhi), hStyleConRight);


                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.truyLanh), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapKhac ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongThanhTich1 ?? 0), hStyleConRight);
                        //tổng thu nhập
                        tongThuNhap = (item.luongThang ?? 0) + (item.phuCapTienAn ?? 0) + (item.phuCapDienThoai ?? 0) + (Convert.ToDouble(item.phuCapDiLaiNew ?? 0))
                           + (item.phuCapThuHut ?? 0) + (item.phuCapThamNien ?? 0) + (item.congTacPhi ?? 0) + (item.sinhHoatPhi ?? 0) + (item.truyLanh ?? 0)
                           + (item.phuCapKhac ?? 0) + (item.luongThanhTich1 ?? 0);

                        tongThuNhapSum = tongThuNhapSum + Convert.ToDecimal(tongThuNhap);


                        //TongThucLanh
                        LuongTheoNgayCong = (item.luongThang ?? 0);
                        TongTienPhuCap = (item.phuCapTienAn ?? 0) + (item.phuCapDienThoai ?? 0) + (item.phuCapThuHut ?? 0) + (Convert.ToDouble(item.phuCapDiLaiNew ?? 0));

                        //var TongThuNhap = LuongTheoNgayCong + TongTienPhuCap + (item.congTacPhi ?? 0) + (item.sinhHoatPhi ?? 0);

                        TongKhauTru = (item.baoHiemXH ?? 0) + (item.baoHiemYTe ?? 0) + (item.baoHiemTN ?? 0) + (item.thue ?? 0) + (item.doanPhi ?? 0) + (item.truyThu ?? 0) + (item.dangPhi ?? 0);
                        if (item.phuCapDiLai == 2)
                        {
                            var TruBaoHiem = (item.baoHiemXH ?? 0) + (item.baoHiemYTe ?? 0) + (item.baoHiemTN ?? 0);
                            TongKhauTru = TongKhauTru - TruBaoHiem;
                        }
                        if (item.phuCapTienXang == 2)
                        {
                            TongKhauTru = TongKhauTru - (item.thue ?? 0);
                        }
                        //var TongPhuCapTL = (item.truyLanh ?? 0) - (item.truyThu ?? 0) - (item.dangPhi ?? 0) ;
                        TongThucLanh = tongThuNhap - TongKhauTru; //+ TongPhuCapTL;
                        tongThucLanhSum = tongThucLanhSum + Convert.ToDecimal(TongThucLanh);
                        tongThuNhapChiuThue = tongThuNhap - (item.phuCapTienAn ?? 0) - (item.phuCapDienThoai ?? 0) - (item.luongThanhTich1 ?? 0);
                        SumtongThuNhapChiuThue = SumtongThuNhapChiuThue + Convert.ToDecimal(tongThuNhapChiuThue);

                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", tongThuNhap), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", tongThuNhapChiuThue), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.soNguoiPhuThuoc), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.giamTruNguoiPhuThuoc), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.giamTruBanThan), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.tongThuNhapChiuThue), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.baoHiemXH ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.baoHiemYTe ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.baoHiemTN ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.doanPhi ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.thue), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.truyThu), hStyleConRight);

                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.dangPhi), hStyleConRight);

                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", TongThucLanh), hStyleConRight);

                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.tamUng ?? 0), hStyleConRight);

                        //ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongThanhTich2 ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongThanhTich3 ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.soNgayPhepTang), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.thucLanh ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongDongBaoHiem * 17.5 / 100), styleCellSumary);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongDongBaoHiem * 3 / 100), styleCellSumary);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongDongBaoHiem * 1 / 100), styleCellSumary);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.soNgayQuet), styleCellSumary);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.truyLanhThangTruoc ?? 0), styleCellSumary);


                    }
                }


                #region dòng tổng cộng
                idRowStart++;
                int demT = 0;
                int tongSoNV = data.Count();
                Row rowT = sheet.CreateRow(idRowStart);
                CellRangeAddress cellRangeAddressT = new CellRangeAddress(idRowStart, idRowStart, demT, demT + 7);
                sheet.AddMergedRegion(cellRangeAddressT);
                ReportHelperExcel.SetAlignment(rowT, demT++, "Tổng cộng: " + String.Format("{0:#,##0}", tongSoNV) + " nhân viên", styleheadedColumnTable);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConCenter);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConCenter);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.tongLuong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapLuong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.khoanBoSungLuong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongChoViec)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongDongBaoHiem)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.ngayCongChuan)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.tongNgayCong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayNghiPhep)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayNghiBu)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayNghiLe)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayNghiKhacDuocHuongLuong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayNghiKhongLuong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayTangCaThuong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayTangCaChuNhat)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayTangCaNgayLe)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongThang)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapTienAn)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapDiLaiNew)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapDienThoai)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", 0), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapThuHut)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapThamNien)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.sinhHoatPhi)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.congTacPhi)), styleCellSumary);


                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.truyLanh)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapKhac)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongThanhTich1)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", (tongThuNhapSum / 2)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", SumtongThuNhapChiuThue), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.soNguoiPhuThuoc)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.giamTruNguoiPhuThuoc)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.giamTruBanThan)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.tongThuNhapChiuThue)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.baoHiemXH)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.baoHiemYTe)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.baoHiemTN)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.doanPhi)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.thue)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.truyThu)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.dangPhi)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", tongThucLanhSum), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.tamUng)), styleCellSumary);
                //ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongThanhTich2)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongThanhTich3)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.soNgayPhepTang)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.thucLanh)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongDongBaoHiem) * 17.5 / 100), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongDongBaoHiem) * 3 / 100), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongDongBaoHiem) * 1 / 100), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.soNgayQuet)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.truyLanhThangTruoc ?? 0)), styleCellSumary);
                #endregion

                //idRowStart = idRowStart + 2;
                //string cellFooterGD = "GIÁM ĐỐC NHÂN SỰ-QTVP";
                //var titleCellFooterGD = HSSFCellUtil.CreateCell(sheet.CreateRow(idRowStart), 4, cellFooterGD.ToUpper());
                //titleCellFooterGD.CellStyle = styleTitle;

                //string cellFooterKT = "PHÒNG TÀI CHÍNH - KẾ TOÁN";
                //var titleCellFooterKT = HSSFCellUtil.CreateCell(sheet.GetRow(idRowStart), 21, cellFooterKT.ToUpper());
                //titleCellFooterKT.CellStyle = styleTitle;

                //string cellFooterTGD = "TỔNG GIÁM ĐỐC";
                //var titleCellFooterTGD = HSSFCellUtil.CreateCell(sheet.GetRow(idRowStart), 42, cellFooterTGD.ToUpper());
                //titleCellFooterTGD.CellStyle = styleTitle;

                var stream = new MemoryStream();
                workbook.Write(stream);

                Response.ContentType = "application/vnd.ms-excel";
                Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", filename));
                Response.Clear();

                Response.BinaryWrite(stream.GetBuffer());
                Response.End();
            }
            catch
            {

            }

        }
        public void XuatFileBLNVWindowLA(int thang, int nam, string maPhongBan, string qSearch)
        {

            try
            {
                #region Role user
                permission = GetPermission(MCV, BangPhanQuyen.QuyenIn);
                if (!permission.HasValue)
                    return;
                if (!permission.Value)
                    return;
                #endregion

                var filename = "";
                var virtualPath = HttpRuntime.AppDomainAppVirtualPath;

                var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplate.xls"), FileMode.Open, FileAccess.Read);

                var workbook = new HSSFWorkbook(fileStream, true);
                filename += "BangLuongNV_" + nam + "_" + thang + ".xls";


                var sheet = workbook.GetSheet("danhsachnhanvien");


                #region format style excel
                /*style title start*/
                //tạo font cho các title
                //font tiêu đề 
                HSSFFont hFontTieuDe = (HSSFFont)workbook.CreateFont();
                hFontTieuDe.FontHeightInPoints = 18;
                hFontTieuDe.Boldweight = 100 * 10;
                hFontTieuDe.FontName = "Times New Roman";
                hFontTieuDe.Color = HSSFColor.BLUE.index;

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
                //font chứ hoa đậm
                HSSFFont hFontNommalUpperRED = (HSSFFont)workbook.CreateFont();
                hFontNommalUpperRED.Boldweight = (short)FontBoldWeight.BOLD;
                hFontNommalUpperRED.Color = HSSFColor.RED.index;
                hFontNommalUpperRED.FontName = "Times New Roman";

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

                //style sum cell
                var styleCellSumary = workbook.CreateCellStyle();
                styleCellSumary.SetFont(hFontNommalUpper);
                styleCellSumary.WrapText = true;
                styleCellSumary.BorderBottom = CellBorderType.THIN;
                styleCellSumary.BorderLeft = CellBorderType.THIN;
                styleCellSumary.BorderRight = CellBorderType.THIN;
                styleCellSumary.BorderTop = CellBorderType.THIN;
                styleCellSumary.VerticalAlignment = VerticalAlignment.CENTER;
                styleCellSumary.Alignment = HorizontalAlignment.RIGHT;
                //style sum cell
                var styleCellSumaryTen = workbook.CreateCellStyle();
                styleCellSumaryTen.SetFont(hFontNommalUpper);
                styleCellSumaryTen.WrapText = true;
                styleCellSumaryTen.BorderBottom = CellBorderType.THIN;
                styleCellSumaryTen.BorderLeft = CellBorderType.THIN;
                styleCellSumaryTen.BorderRight = CellBorderType.THIN;
                styleCellSumaryTen.BorderTop = CellBorderType.THIN;
                styleCellSumaryTen.VerticalAlignment = VerticalAlignment.CENTER;
                styleCellSumaryTen.Alignment = HorizontalAlignment.LEFT;

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

                string cellTenCty = "CÔNG TY CỔ PHẦN TV.WINDOW";
                var titleCellCty = HSSFCellUtil.CreateCell(sheet.CreateRow(0), 0, cellTenCty.ToUpper());
                titleCellCty.CellStyle = styleTitle;

                //string cellEnd1 = "Bảng Lương Tháng " + thang + " năm " + nam;
                //var titleCellEnd1 = HSSFCellUtil.CreateCell(sheet.GetRow(0), 43, cellEnd1.ToUpper());
                //titleCellEnd1.CellStyle = styleTitle;

                string rowtitle = "Bảng Lương Công Ty Tháng " + thang + " năm " + nam;
                var titleCell = HSSFCellUtil.CreateCell(sheet.CreateRow(firstRowNumber), 15, rowtitle.ToUpper());
                titleCell.CellStyle = styleTitle;

                //string cellEnd2 = "";
                //var titleCellEnd2 = HSSFCellUtil.CreateCell(sheet.GetRow(1), 43, cellEnd2.ToUpper());
                //titleCellEnd2.CellStyle = styleTitle;

                ++firstRowNumber;

                var list1 = new List<string>();
                list1.Add("STT");
                list1.Add("Mã nhân viên");
                list1.Add("Họ tên");
                list1.Add("Công ty");
                list1.Add("Chức vụ");
                list1.Add("Phòng ban");
                list1.Add("Bậc");
                list1.Add("Mức");
                list1.Add("Tổng lương");
                list1.Add("Lương cơ bản");
                list1.Add("Lương thành tích");
                list1.Add("Lương kiêm nhiệm");
                list1.Add("Lương đóng BHXH");
                list1.Add("Ngày công chuẩn");
                list1.Add("Ngày công tính lương");
                list1.Add("Ngày nghỉ");// 
                list1.Add("");//
                list1.Add("");//
                list1.Add("");//
                list1.Add("");//
                list1.Add("Tăng ca");//
                list1.Add("");//
                list1.Add("");   //         
                list1.Add("Lương CB theo ngày công thực tế");
                list1.Add("Các khoản phụ cấp");
                list1.Add("");//
                list1.Add("");//
                list1.Add("");//
                list1.Add("");//
                list1.Add("");//

                list1.Add("Phụ cấp khác");
                list1.Add("Công tác phí");

                list1.Add("Truy lãnh");
                list1.Add("Thu nhập khác");
                list1.Add("Tiền cơm công tác");
                list1.Add("Tổng thu nhập");

                list1.Add("Tổng thu nhập chịu thuế");

                list1.Add("Số người phụ thuộc");
                list1.Add("Các khoản giảm trừ không tính thuế TNCN");
                list1.Add("");
                list1.Add("Thu nhập tính thuế TNCN");
                list1.Add("Các khoản trích trừ vào lương");
                list1.Add("");
                list1.Add("");
                list1.Add("Quỹ nội bộ (1%)");
                list1.Add("Thuế TNCN (Tạm thu)");
                list1.Add("Truy thu");

                list1.Add("Truy thu bảo hiểm");
                list1.Add("Tổng thu nhập thực tế");

                list1.Add("Tạm ứng");
                //list1.Add("Quyết toán lương tháng 1");
                list1.Add("Truy thu thuế TNCN 2017");
                list1.Add("Truy lãnh thuế TNCN 2017");
                list1.Add("THỰC LÃNH");
                list1.Add("DN đóng BHXH");
                list1.Add("");
                list1.Add("Số ngày quét");
                list1.Add("Truy lãnh tháng trước");
                var list2 = new List<string>();
                list2.Add("STT");
                list2.Add("Mã nhân viên");
                list2.Add("Họ tên");
                list2.Add("Công ty");
                list2.Add("Chức vụ");
                list2.Add("Phòng ban");
                list2.Add("Bậc");
                list2.Add("Mức");
                list2.Add("Tổng lương");
                list2.Add("Lương cơ bản");
                list2.Add("Lương thành tích");
                list2.Add("Lương kiêm nhiệm");
                list2.Add("Lương đóng BHXH");
                list2.Add("Ngày công chuẩn");
                list2.Add("Ngày công tính lương");
                list2.Add("Phép (P)");
                list2.Add("Nghỉ bù (NB)");
                list2.Add("Lễ (L)");
                list2.Add("Hưởng lương (R)");
                list2.Add("Không lương (RO)");
                list2.Add("Ngày thường");
                list2.Add("Ngày CN");
                list2.Add("Ngày lễ");
                list2.Add("Lương CB theo ngày công thực tế");
                list2.Add("Tiền ăn giữa ca");
                list2.Add("Đi lại");
                list2.Add("Điện thoại");
                list2.Add("Xăng xe");
                list2.Add("Thu hút");
                list2.Add("Thâm niên");

                //list2.Add("Sinh hoạt phí");
                list2.Add("Phụ cấp khác (bao gồm p/c chức danh)");
                list2.Add("Công tác phí");

                list2.Add("Truy lãnh");
                list2.Add("Thu nhập khác");
                list2.Add("Tiền cơm công tác");
                list2.Add("Tổng thu nhập");
                list2.Add("Tổng thu nhập chịu thuế");
                list2.Add("Số người phụ thuộc");
                list2.Add("Gia cảnh");
                list2.Add("Bản thân");
                list2.Add("Thu nhập tính thuế TNCN");
                list2.Add("BHXH (8%)");
                list2.Add("BHYT (1.5%)");
                list2.Add("BHTN (1%)");
                list2.Add("Quỹ nội bộ (1%)");
                list2.Add("Thuế TNCN (Tạm thu)");
                list2.Add("Truy thu");

                list2.Add("Truy thu bảo hiểm");
                list2.Add("Tổng thu nhập thực tế");

                list2.Add("Tạm ứng");
                //list2.Add("Quyết toán lương tháng 1");
                list2.Add("Truy thu thuế TNCN 2017");
                list2.Add("Truy lãnh thuế TNCN 2017");
                list2.Add("THỰC LÃNH");
                list2.Add("BHXH (17.5%)");
                list2.Add("BHYT (3%)");
                list2.Add("BHTN (1%)");
                list2.Add("Số ngày quét");
                list2.Add("Truy lãnh tháng trước");
                var list3 = new List<string>();
                for (var col = 1; col <= 55; col++)
                {
                    list3.Add("" + col + "");
                }

                var idRowStart = 2; // bat dau o dong thu 4
                var headerRow = sheet.CreateRow(idRowStart);
                int rowend = idRowStart;
                ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);
                idRowStart++;
                var headerRow1 = sheet.CreateRow(idRowStart);
                ReportHelperExcel.CreateHeaderRow(headerRow1, 0, styleheadedColumnTable, list2);

                idRowStart++;
                var headerRow2 = sheet.CreateRow(idRowStart);
                ReportHelperExcel.CreateHeaderRow(headerRow2, 0, styleheadedColumnTable, list3);
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 53, 55));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 52, 52));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 51, 51));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 50, 50));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 49, 49));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 48, 48));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 47, 47));

                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 46, 46));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 45, 45));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 44, 44));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 41, 43));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 40, 40));

                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 38, 39));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 33, 33));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 37, 37));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 36, 36));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 35, 35));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 34, 34));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 33, 33));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 32, 32));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 31, 31));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 30, 30));
                //sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 25, 30));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 24, 29));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 23, 23));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 20, 22));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 15, 19));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 14, 14));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 13, 13));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 12, 12));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 11, 11));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 10, 10));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 9, 9));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 8, 8));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 7, 7));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 6, 6));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 5, 5));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 4, 4));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 3, 3));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 2, 2));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 1, 1));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 0, 0));



                sheet.SetColumnWidth(0, 8 * 210);
                sheet.SetColumnWidth(1, 30 * 210);
                sheet.SetColumnWidth(2, 30 * 310);
                sheet.SetColumnWidth(3, 30 * 210);
                sheet.SetColumnWidth(4, 8 * 210);
                sheet.SetColumnWidth(5, 15 * 210);
                sheet.SetColumnWidth(6, 15 * 210);
                sheet.SetColumnWidth(7, 15 * 210);
                sheet.SetColumnWidth(8, 15 * 210);
                sheet.SetColumnWidth(9, 15 * 210);
                sheet.SetColumnWidth(10, 15 * 210);
                sheet.SetColumnWidth(11, 15 * 210);

                sheet.SetColumnWidth(12, 15 * 210);
                sheet.SetColumnWidth(13, 15 * 210);
                sheet.SetColumnWidth(14, 15 * 210);
                sheet.SetColumnWidth(15, 10 * 210);
                sheet.SetColumnWidth(16, 15 * 210);
                sheet.SetColumnWidth(17, 15 * 210);
                sheet.SetColumnWidth(18, 15 * 210);
                sheet.SetColumnWidth(19, 15 * 210);
                sheet.SetColumnWidth(20, 20 * 210);
                sheet.SetColumnWidth(21, 15 * 210);
                sheet.SetColumnWidth(22, 15 * 210);
                sheet.SetColumnWidth(23, 15 * 210);

                sheet.SetColumnWidth(24, 15 * 210);
                sheet.SetColumnWidth(25, 15 * 210);
                sheet.SetColumnWidth(26, 15 * 210);
                sheet.SetColumnWidth(27, 15 * 210);
                sheet.SetColumnWidth(28, 15 * 210);
                sheet.SetColumnWidth(29, 15 * 210);
                sheet.SetColumnWidth(30, 15 * 210);
                sheet.SetColumnWidth(31, 15 * 210);
                sheet.SetColumnWidth(32, 15 * 210);
                sheet.SetColumnWidth(33, 15 * 210);
                sheet.SetColumnWidth(34, 15 * 210);
                sheet.SetColumnWidth(35, 15 * 210);

                sheet.SetColumnWidth(36, 15 * 210);
                sheet.SetColumnWidth(37, 15 * 210);
                sheet.SetColumnWidth(38, 15 * 210);
                sheet.SetColumnWidth(39, 15 * 210);
                sheet.SetColumnWidth(40, 15 * 210);
                sheet.SetColumnWidth(41, 15 * 210);
                sheet.SetColumnWidth(42, 15 * 210);
                sheet.SetColumnWidth(43, 15 * 210);
                sheet.SetColumnWidth(44, 15 * 210);
                sheet.SetColumnWidth(45, 15 * 210);
                sheet.SetColumnWidth(46, 15 * 210);
                sheet.SetColumnWidth(47, 15 * 210);
                sheet.SetColumnWidth(48, 15 * 210);
                sheet.SetColumnWidth(49, 15 * 210);
                sheet.SetColumnWidth(50, 15 * 210);
                sheet.SetColumnWidth(51, 20 * 400);
                sheet.SetColumnWidth(52, 20 * 400);
                sheet.SetColumnWidth(53, 20 * 400);
                sheet.SetColumnWidth(54, 20 * 400);
                sheet.SetColumnWidth(55, 20 * 400);
                sheet.SetColumnWidth(56, 20 * 400);
                var data = nhanSuContext.sp_NS_BangLuongNhanVien_TVWINDOWLA(maPhongBan, thang, nam, qSearch).ToList();

                var lstPhongBan = nhanSuContext.sp_NS_BangLuongNhanVien_TVWINDOWLA(maPhongBan, thang, nam, qSearch).Select(d => d.phongBan).Distinct().ToList();
                var stt = 0;
                var sttPB = 0;
                int dem = 0;
                decimal tongThuNhapSum = 0;
                decimal tongThucLanhSum = 0;
                decimal SumtongThuNhapChiuThue = 0;
                foreach (var itemPB in lstPhongBan)
                {

                    dem = 0;
                    stt = 0;
                    idRowStart++;
                    decimal tongSo = data.Where(d => d.phongBan == itemPB).Count();
                    rowC = sheet.CreateRow(idRowStart);
                    ReportHelperExcel.SetAlignment(rowC, dem++, (++sttPB).ToString(), styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", tongSo) + " nhân viên", styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, itemPB, styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, "", styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, "", styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, "", styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, "", styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, "", styleCellSumaryTen);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.tongLuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapLuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.khoanBoSungLuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongChoViec)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongDongBaoHiem)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.ngayCongChuan)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.tongNgayCong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayNghiPhep)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayNghiBu)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayNghiLe)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayNghiKhacDuocHuongLuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayNghiKhongLuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayTangCaThuong)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayTangCaChuNhat)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayTangCaNgayLe)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThang)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapTienAn)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDiLaiNew ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDienThoai)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", 0), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapThuHut)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapThamNien)), styleCellSumary);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.sinhHoatPhi)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.congTacPhi)), styleCellSumary);


                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.truyLanh)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapKhac ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThanhTich1 ?? 0)), styleCellSumary);
                    //tổng thu nhập
                    var tongThuNhap =
                        (data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThang ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapTienAn ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDienThoai ?? 0))
                        + (Convert.ToDouble(data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDiLaiNew ?? 0)))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapThuHut ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapThamNien ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.congTacPhi ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.sinhHoatPhi ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.truyLanh ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapKhac ?? 0)) + (data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThanhTich1 ?? 0));

                    tongThuNhapSum = tongThuNhapSum + Convert.ToDecimal(tongThuNhap);
                    //TongThucLanh
                    var LuongTheoNgayCong = (data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThang ?? 0));
                    var TongTienPhuCap = (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapTienAn ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDienThoai ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapThuHut ?? 0)) + (Convert.ToDouble(data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDiLaiNew ?? 0)));

                    //var TongThuNhap = LuongTheoNgayCong + TongTienPhuCap + (item.congTacPhi ?? 0) + (item.sinhHoatPhi ?? 0);

                    var TongKhauTru = (data.Where(d => d.phongBan == itemPB).Sum(d => d.baoHiemXH ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.baoHiemYTe ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.baoHiemTN ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.thue ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.doanPhi ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.truyThu ?? 0))
                        + (data.Where(d => d.phongBan == itemPB).Sum(d => d.dangPhi ?? 0));
                    foreach (var item in data.Where(d => d.phongBan == itemPB))
                    {
                        if (item.phuCapDiLai == 2)
                        {
                            var TruBaoHiem = (item.baoHiemXH ?? 0) + (item.baoHiemYTe ?? 0) + (item.baoHiemTN ?? 0);
                            TongKhauTru = TongKhauTru - TruBaoHiem;
                        }
                        if (item.phuCapTienXang == 2)
                        {
                            TongKhauTru = TongKhauTru - (item.thue ?? 0);
                        }
                    }
                    //var TongPhuCapTL = (item.truyLanh ?? 0) - (item.truyThu ?? 0) - (item.dangPhi ?? 0) ;
                    var TongThucLanh = tongThuNhap - TongKhauTru; //+ TongPhuCapTL;



                    var tongThuNhapChiuThue = tongThuNhap - (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapTienAn ?? 0)) - (data.Where(d => d.phongBan == itemPB).Sum(d => d.phuCapDienThoai ?? 0))
                        - (data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThanhTich1 ?? 0));
                    SumtongThuNhapChiuThue = SumtongThuNhapChiuThue + Convert.ToDecimal(tongThuNhapChiuThue);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", tongThuNhap), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", tongThuNhapChiuThue), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNguoiPhuThuoc)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.giamTruNguoiPhuThuoc)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.giamTruBanThan)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.tongThuNhapChiuThue)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.baoHiemXH ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.baoHiemYTe ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.baoHiemTN ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.doanPhi ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.thue)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.truyThu)), styleCellSumary);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.dangPhi)), styleCellSumary);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", TongThucLanh), styleCellSumary);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.tamUng ?? 0)), styleCellSumary);

                    //ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongThanhTich2 ?? 0), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongThanhTich3 ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayPhepTang)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.thucLanh ?? 0)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongDongBaoHiem ?? 0) * 17.5 / 100), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongDongBaoHiem ?? 0) * 3 / 100), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.luongDongBaoHiem ?? 0) * 1 / 100), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.soNgayQuet)), styleCellSumary);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", data.Where(d => d.phongBan == itemPB).Sum(d => d.truyLanhThangTruoc ?? 0)), styleCellSumary);

                    // nhan vien
                    foreach (var item in data.Where(d => d.phongBan == itemPB))
                    {
                        dem = 0;

                        idRowStart++;
                        rowC = sheet.CreateRow(idRowStart);
                        ReportHelperExcel.SetAlignment(rowC, dem++, sttPB + "." + (++stt).ToString(), hStyleConCenter);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.maNhanVien, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.hoTen, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.tenKhoiTinhLuong, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.chucVu, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.phongBan, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.bac), hStyleConCenter);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.hinhThucLuong, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.tongLuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapLuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.khoanBoSungLuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongChoViec), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongDongBaoHiem), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.ngayCongChuan), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.tongNgayCong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayNghiPhep), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayNghiBu), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayNghiLe), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayNghiKhacDuocHuongLuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayNghiKhongLuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayTangCaThuong), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayTangCaChuNhat), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", item.soNgayTangCaNgayLe), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongThang), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapTienAn), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapDiLaiNew ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapDienThoai), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapThuHut), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapThamNien), hStyleConRight);

                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.sinhHoatPhi), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.congTacPhi), hStyleConRight);


                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.truyLanh), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.phuCapKhac ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongThanhTich1 ?? 0), hStyleConRight);
                        //tổng thu nhập
                        tongThuNhap = (item.luongThang ?? 0) + (item.phuCapTienAn ?? 0) + (item.phuCapDienThoai ?? 0) + (Convert.ToDouble(item.phuCapDiLaiNew ?? 0))
                           + (item.phuCapThuHut ?? 0) + (item.phuCapThamNien ?? 0) + (item.congTacPhi ?? 0) + (item.sinhHoatPhi ?? 0) + (item.truyLanh ?? 0)
                           + (item.phuCapKhac ?? 0) + (item.luongThanhTich1 ?? 0);

                        tongThuNhapSum = tongThuNhapSum + Convert.ToDecimal(tongThuNhap);


                        //TongThucLanh
                        LuongTheoNgayCong = (item.luongThang ?? 0);
                        TongTienPhuCap = (item.phuCapTienAn ?? 0) + (item.phuCapDienThoai ?? 0) + (item.phuCapThuHut ?? 0) + (Convert.ToDouble(item.phuCapDiLaiNew ?? 0));

                        //var TongThuNhap = LuongTheoNgayCong + TongTienPhuCap + (item.congTacPhi ?? 0) + (item.sinhHoatPhi ?? 0);

                        TongKhauTru = (item.baoHiemXH ?? 0) + (item.baoHiemYTe ?? 0) + (item.baoHiemTN ?? 0) + (item.thue ?? 0) + (item.doanPhi ?? 0) + (item.truyThu ?? 0) + (item.dangPhi ?? 0);
                        if (item.phuCapDiLai == 2)
                        {
                            var TruBaoHiem = (item.baoHiemXH ?? 0) + (item.baoHiemYTe ?? 0) + (item.baoHiemTN ?? 0);
                            TongKhauTru = TongKhauTru - TruBaoHiem;
                        }
                        if (item.phuCapTienXang == 2)
                        {
                            TongKhauTru = TongKhauTru - (item.thue ?? 0);
                        }
                        //var TongPhuCapTL = (item.truyLanh ?? 0) - (item.truyThu ?? 0) - (item.dangPhi ?? 0) ;
                        TongThucLanh = tongThuNhap - TongKhauTru; //+ TongPhuCapTL;
                        tongThucLanhSum = tongThucLanhSum + Convert.ToDecimal(TongThucLanh);
                        tongThuNhapChiuThue = tongThuNhap - (item.phuCapTienAn ?? 0) - (item.phuCapDienThoai ?? 0) - (item.luongThanhTich1 ?? 0);
                        SumtongThuNhapChiuThue = SumtongThuNhapChiuThue + Convert.ToDecimal(tongThuNhapChiuThue);

                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.##}", tongThuNhap), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", tongThuNhapChiuThue), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.soNguoiPhuThuoc), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.giamTruNguoiPhuThuoc), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.giamTruBanThan), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.tongThuNhapChiuThue), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.baoHiemXH ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.baoHiemYTe ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.baoHiemTN ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.doanPhi ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.thue), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.truyThu), hStyleConRight);

                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.dangPhi), hStyleConRight);

                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", TongThucLanh), hStyleConRight);

                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.tamUng ?? 0), hStyleConRight);

                        //ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongThanhTich2 ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongThanhTich3 ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.soNgayPhepTang), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.thucLanh ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongDongBaoHiem * 17.5 / 100), styleCellSumary);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongDongBaoHiem * 3 / 100), styleCellSumary);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.luongDongBaoHiem * 1 / 100), styleCellSumary);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.soNgayQuet), styleCellSumary);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", item.truyLanhThangTruoc ?? 0), styleCellSumary);

                    }
                }


                #region dòng tổng cộng
                idRowStart++;
                int demT = 0;
                int tongSoNV = data.Count();
                Row rowT = sheet.CreateRow(idRowStart);
                CellRangeAddress cellRangeAddressT = new CellRangeAddress(idRowStart, idRowStart, demT, demT + 7);
                sheet.AddMergedRegion(cellRangeAddressT);
                ReportHelperExcel.SetAlignment(rowT, demT++, "Tổng cộng: " + String.Format("{0:#,##0}", tongSoNV) + " nhân viên", styleheadedColumnTable);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConCenter);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConCenter);
                ReportHelperExcel.SetAlignment(rowT, demT++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.tongLuong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapLuong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.khoanBoSungLuong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongChoViec)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongDongBaoHiem)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.ngayCongChuan)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.tongNgayCong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayNghiPhep)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayNghiBu)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayNghiLe)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayNghiKhacDuocHuongLuong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayNghiKhongLuong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayTangCaThuong)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayTangCaChuNhat)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0.##}", data.Sum(s => s.soNgayTangCaNgayLe)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongThang)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapTienAn)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapDiLaiNew)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapDienThoai)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", 0), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapThuHut)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapThamNien)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.sinhHoatPhi)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.congTacPhi)), styleCellSumary);


                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.truyLanh)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.phuCapKhac)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongThanhTich1)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", (tongThuNhapSum / 2)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", SumtongThuNhapChiuThue), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.soNguoiPhuThuoc)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.giamTruNguoiPhuThuoc)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.giamTruBanThan)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.tongThuNhapChiuThue)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.baoHiemXH)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.baoHiemYTe)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.baoHiemTN)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.doanPhi)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.thue)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.truyThu)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.dangPhi)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", tongThucLanhSum), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.tamUng)), styleCellSumary);
                //ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongThanhTich2)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongThanhTich3)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.soNgayPhepTang)), styleCellSumary);

                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.thucLanh)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongDongBaoHiem) * 17.5 / 100), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongDongBaoHiem) * 3 / 100), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.luongDongBaoHiem) * 1 / 100), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.soNgayQuet)), styleCellSumary);
                ReportHelperExcel.SetAlignment(rowT, demT++, String.Format("{0:#,##0}", data.Sum(s => s.truyLanhThangTruoc ?? 0)), styleCellSumary);
                #endregion

                //idRowStart = idRowStart + 2;
                //string cellFooterGD = "GIÁM ĐỐC NHÂN SỰ-QTVP";
                //var titleCellFooterGD = HSSFCellUtil.CreateCell(sheet.CreateRow(idRowStart), 4, cellFooterGD.ToUpper());
                //titleCellFooterGD.CellStyle = styleTitle;

                //string cellFooterKT = "PHÒNG TÀI CHÍNH - KẾ TOÁN";
                //var titleCellFooterKT = HSSFCellUtil.CreateCell(sheet.GetRow(idRowStart), 21, cellFooterKT.ToUpper());
                //titleCellFooterKT.CellStyle = styleTitle;

                //string cellFooterTGD = "TỔNG GIÁM ĐỐC";
                //var titleCellFooterTGD = HSSFCellUtil.CreateCell(sheet.GetRow(idRowStart), 42, cellFooterTGD.ToUpper());
                //titleCellFooterTGD.CellStyle = styleTitle;

                var stream = new MemoryStream();
                workbook.Write(stream);

                Response.ContentType = "application/vnd.ms-excel";
                Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", filename));
                Response.Clear();

                Response.BinaryWrite(stream.GetBuffer());
                Response.End();
            }
            catch
            {

            }

        }

        // End Xuat File
        // Xuat File Bang Luong Nhan Vien theo bộ phận
        public void XuatFileBLNVBoPhan(int thang, int nam)
        {

            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenIn);
            if (!permission.HasValue)
                return;
            if (!permission.Value)
                return;
            #endregion

            var filename = "";
            var virtualPath = HttpRuntime.AppDomainAppVirtualPath;

            filename += "BangLuongNVBoPhan_" + nam + "_" + thang + ".xlsx";

            using (ExcelPackage package = new ExcelPackage())
            {
                //Create a sheet
                package.Workbook.Worksheets.Add("BangLuongNVBoPhan_" + nam + "_" + thang);
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                //Header
                //insert từ dòng nào, bao nhiêu row
                var rowFrom = 1;
                worksheet.InsertRow(rowFrom, 1);
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Phòng ban";
                worksheet.Cells[1, 3].Value = "Tháng";
                worksheet.Cells[1, 4].Value = "Năm";
                worksheet.Cells[1, 5].Value = "Tổng lương";
                worksheet.Cells[1, 6].Value = "Bảo hiểm";
                worksheet.Cells[1, 7].Value = "Thuế";
                worksheet.Cells[1, 8].Value = "Truy thu bảo hiểm";
                worksheet.Cells[1, 9].Value = "Truy thu";
                worksheet.Cells[1, 10].Value = "Truy lãnh";
                worksheet.Cells[1, 11].Value = "Thực lãnh";
                worksheet.Column(2).Width = 35;
                worksheet.Column(3).Width = 35;
                worksheet.Column(4).Width = 20;
                worksheet.Column(5).Width = 20;
                worksheet.Column(6).Width = 20;
                worksheet.Column(7).Width = 20;
                worksheet.Column(8).Width = 20;
                worksheet.Column(9).Width = 20;
                worksheet.Column(10).Width = 20;
                worksheet.Column(11).Width = 20;
                //// Formatting style of the header
                using (var range = worksheet.Cells[1, 1, 2, 11])
                {
                    // Setting bold font
                    range.Style.Font.Bold = true;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    // Setting fill type solid
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    // Setting background color dark blue
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    // Setting font color
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }



                #region
                //Body
                //var data = nhanSuContext.sp_NS_BangLuongTheoBoPhan(thang, nam).ToList();

                //if (data != null && data.Count > 0)
                //{
                //    var countSTT = 1;
                //    foreach (var item in data)
                //    {

                //        rowFrom = rowFrom + 1;
                //        worksheet.InsertRow(rowFrom, 1);
                //        worksheet.Cells[rowFrom, 1].Value = countSTT++;
                //        worksheet.Cells[rowFrom, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                //        worksheet.Cells[rowFrom, 1].Style.Font.Bold = true;
                //        worksheet.Cells[rowFrom, 2].Value = item.boPhanTinhLuong;
                //        worksheet.Cells[rowFrom, 3].Value = item.thang;
                //        worksheet.Cells[rowFrom, 4].Value = item.nam;


                //        worksheet.Cells[rowFrom, 5].Value = item.tongLuong;
                //        worksheet.Cells[rowFrom, 5].Style.Numberformat.Format = "#,##0.000";

                //        worksheet.Cells[rowFrom, 6].Value = item.baoHiem;
                //        worksheet.Cells[rowFrom, 6].Style.Numberformat.Format = "#,##0.000";

                //        worksheet.Cells[rowFrom, 7].Value = item.thue;
                //        worksheet.Cells[rowFrom, 7].Style.Numberformat.Format = "#,##0";

                //        worksheet.Cells[rowFrom, 8].Value = item.truyThuBaoHiem;
                //        worksheet.Cells[rowFrom, 8].Style.Numberformat.Format = "#,##0.000";

                //        worksheet.Cells[rowFrom, 9].Value = item.truyThu;
                //        worksheet.Cells[rowFrom, 9].Style.Numberformat.Format = "#,##0.000";

                //        worksheet.Cells[rowFrom, 10].Value = item.truyLanh;
                //        worksheet.Cells[rowFrom, 10].Style.Numberformat.Format = "#,##0.000";

                //        worksheet.Cells[rowFrom, 11].Value = item.thucLanh;
                //        worksheet.Cells[rowFrom, 11].Style.Numberformat.Format = "#,##0.000";
                //    }
                //}

                #endregion

                //Generate A File
                Byte[] bin = package.GetAsByteArray();

                Response.BinaryWrite(bin);
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", string.Format("attachment;filename={0}", filename));
            }
        }

        // End Xuat File
        // Xuat File Bang Luong Nhan Vien theo bộ phận
        public void XuatFileBLNN(int thang, int nam)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenIn);
            if (!permission.HasValue)
                return;
            if (!permission.Value)
                return;
            #endregion

            string maPhongBan = "";
            string qSearch = "";
            var filename = "";
            var virtualPath = HttpRuntime.AppDomainAppVirtualPath;

            filename += "BangLuongChuyenNN_" + nam + "_" + thang + ".xlsx";

            using (ExcelPackage package = new ExcelPackage())
            {
                //Create a sheet
                package.Workbook.Worksheets.Add("BangLuongChuyenNN_" + nam + "_" + thang);
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                //Header
                //insert từ dòng nào, bao nhiêu row
                var rowFrom = 1;
                worksheet.InsertRow(rowFrom, 1);
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Mã nhân viên";
                worksheet.Cells[1, 3].Value = "Họ tên";
                worksheet.Cells[1, 4].Value = "Bộ phận tính lương";
                worksheet.Cells[1, 5].Value = "Số tài khoản";
                worksheet.Cells[1, 6].Value = "Tên ngân hàng";
                worksheet.Cells[1, 7].Value = "Số CMND";
                worksheet.Cells[1, 8].Value = "Thực lãnh";

                worksheet.Column(2).Width = 35;
                worksheet.Column(3).Width = 35;
                worksheet.Column(4).Width = 20;
                worksheet.Column(5).Width = 20;
                worksheet.Column(6).Width = 20;
                worksheet.Column(7).Width = 20;
                worksheet.Column(8).Width = 20;
                //// Formatting style of the header
                using (var range = worksheet.Cells[1, 1, 2, 8])
                {
                    // Setting bold font
                    range.Style.Font.Bold = true;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    // Setting fill type solid
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    // Setting background color dark blue
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    // Setting font color
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }



                #region
                //Body
                var data = nhanSuContext.sp_NS_BangLuongChuyenNganHang(thang, nam, maPhongBan, qSearch).ToList();

                if (data != null && data.Count > 0)
                {
                    var countSTT = 1;
                    foreach (var item in data)
                    {

                        rowFrom = rowFrom + 1;
                        worksheet.InsertRow(rowFrom, 1);
                        worksheet.Cells[rowFrom, 1].Value = countSTT++;
                        worksheet.Cells[rowFrom, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                        worksheet.Cells[rowFrom, 1].Style.Font.Bold = true;
                        worksheet.Cells[rowFrom, 2].Value = item.maNhanVien;
                        worksheet.Cells[rowFrom, 3].Value = item.hoTen.ToString().ToUpper();
                        worksheet.Cells[rowFrom, 4].Value = item.tenKhoiTinhLuong;
                        worksheet.Cells[rowFrom, 5].Value = item.soTaiKhoan;
                        worksheet.Cells[rowFrom, 6].Value = item.tenNganHang;
                        worksheet.Cells[rowFrom, 7].Value = item.soCMND;
                        worksheet.Cells[rowFrom, 8].Value = item.thucLanh;
                        worksheet.Cells[rowFrom, 8].Style.Numberformat.Format = "#,##0.000";
                        worksheet.Cells[rowFrom, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

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

        // End Xuat File

        #region Tạo đề nghị chi lương tự động
        public JsonResult TaoDeNghiChiLuong(int thang, int nam, string maKhoiTaoDeNghiChiLuong)
        {
            try
            {
                var khoiDNCL = nhanSuContext.GetTable<tbl_NS_KhoiTaoDeNghiChiLuong>().Where(d => d.maKhoiDeNghiChiLuong == maKhoiTaoDeNghiChiLuong).FirstOrDefault();
                string giaTri = string.Empty;
                if (khoiDNCL.ghiChu == "TV.WINDOW")
                {
                    giaTri = TaoDeNghiChiLuongTVTD(thang, nam, maKhoiTaoDeNghiChiLuong);
                }               
                return Json(giaTri);
            }
            catch
            {
                return Json("Error");
            }
        }

        public string TaoDeNghiChiLuongTVTD(int thang, int nam, string maKhoiTaoDeNghiChiLuong)
        {
            string flagTT = "DT";
            var listChiLuong = nhanSuContext.sp_NS_TaoDeNghiChiLuong(thang, nam, maKhoiTaoDeNghiChiLuong).ToList();
            var listGroup = listChiLuong.GroupBy(d => new { d.loaiChi }).Select(c => new
            {
                loaiChi = c.Key.loaiChi
            }).ToList();
            foreach (var item in listGroup)
            {
                var flagDNCL = lqThuanViet.tbl_DeNghiChiLuongs.Where(d => d.thang == thang && d.nam == nam && d.loaiChi == item.loaiChi).FirstOrDefault();
                if (flagDNCL == null)
                {
                    flagTT = "";
                    tbl_DeNghiChiLuong deNghi = new tbl_DeNghiChiLuong();
                    deNghi.maPhieu = CheckLetterDNCL("DNCL", GetMaxDeNghiChiLuongTV(), 3);
                    deNghi.thang = thang;
                    deNghi.nam = nam;
                    deNghi.ngayLap = DateTime.Now;
                    deNghi.nguoiLap = GetUser().manv;
                    deNghi.tenNguoiLap = HoVaTen(GetUser().manv);
                    deNghi.loaiChi = item.loaiChi;
                    var listCT = InsertChiTiet(item.loaiChi, deNghi.maPhieu, listChiLuong.Where(d => d.loaiChi == item.loaiChi).ToList());
                    string maCongTrinh = listCT.Select(d => d.maCongTrinh).FirstOrDefault() ?? string.Empty;
                    deNghi.noiDung = "Thanh toán tiền " + item.loaiChi + " tháng " + thang;
                    deNghi.maCongTrinh = maCongTrinh;
                    deNghi.soTienThucChuyen = listCT.Sum(d => d.luongThang);
                    lqThuanViet.tbl_DeNghiChiLuongs.InsertOnSubmit(deNghi);
                    lqThuanViet.tbl_DeNghiChiLuongChiTiets.InsertAllOnSubmit(listCT);
                    lqThuanViet.SubmitChanges();
                }
            }
            return flagTT;
        }

        public List<tbl_DeNghiChiLuongChiTiet> InsertChiTiet(string loaiChi, string maPhieu, List<sp_NS_TaoDeNghiChiLuongResult> lisDNCL)
        {
            List<tbl_DeNghiChiLuongChiTiet> listCT = new List<tbl_DeNghiChiLuongChiTiet>();
            tbl_DeNghiChiLuongChiTiet ct;
            foreach (var item in lisDNCL)
            {
                ct = new tbl_DeNghiChiLuongChiTiet();
                ct.maPhieu = maPhieu;
                ct.tenBoPhanTinhLuong = item.boPhanTinhLuong;
                ct.luongThang = (decimal?)item.soTien ?? 0;
                ct.maCongTrinh = item.maCongTrinh;
                listCT.Add(ct);
            }
            return listCT;
        }

        public string GetMaxDeNghiChiLuongTV()
        {
            return lqThuanViet.tbl_DeNghiChiLuongs.OrderByDescending(d => d.ngayLap).Select(d => d.maPhieu).FirstOrDefault();
        }

        public string CheckLetterDNCL(string preString, string maxValue, int length)
        {
            string yearCurrent = DateTime.Now.Year.ToString().Substring(2, 2);
            string monthCurrent = DateTime.Now.Month.ToString(); // "4"
            //khi thang hien tai nho hon 9 thi cong them "0" vao
            if (Convert.ToInt32(monthCurrent) <= 9)
            {
                monthCurrent = "0" + monthCurrent;
            }
            //Khi tham so select o database la null khoi tao so dau tien
            if (String.IsNullOrEmpty(maxValue))
            {
                string ret = "1";
                while (ret.Length < length)
                {
                    ret = "0" + ret;
                }
                return preString + yearCurrent + monthCurrent + "-" + ret;
            }
            else
            {
                string preStringMax = maxValue.Substring(0, maxValue.IndexOf("-") - 4);
                string maxNumber = maxValue.Substring(maxValue.IndexOf("-") + 1);
                string monthYear = maxValue.Substring(maxValue.IndexOf("-") - 4, 4);
                string monthDb = monthYear.Substring(2, 2); //as "04"

                string stringTemp = maxNumber;
                //Khi thang trong gia tri max bang voi thang create thi cong len cho 1
                if (monthDb == monthCurrent)
                {
                    int strToInt = Convert.ToInt32(maxNumber);
                    maxNumber = Convert.ToString(strToInt + 1);
                    while (maxNumber.Length < stringTemp.Length)
                        maxNumber = "0" + maxNumber;
                }
                else //reset
                {
                    maxNumber = "1";
                    while (maxNumber.Length < stringTemp.Length)
                    {
                        maxNumber = "0" + maxNumber;
                    }
                }

                return preStringMax + yearCurrent + monthCurrent + "-" + maxNumber;
            }
        }
        #endregion


    }
}
