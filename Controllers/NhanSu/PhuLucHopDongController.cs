using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Helper.Common;
using BatDongSan.Models.DanhMuc;
using BatDongSan.Models.HeThong;
using BatDongSan.Models.NhanSu;

namespace BatDongSan.Controllers.NhanSu
{
    public class PhuLucHopDongController : ApplicationController
    {
        private PhanBoLuongModel phanBoLuong;
        private LinqNhanSuDataContext context = new LinqNhanSuDataContext();
        private IList<PhuLucHopDongModel> phuLucs;
        private PhuLucHopDongModel model;
        private tbl_NS_HopDongLaoDong hopDong;
        private tbl_NS_PhuLucHopDong phuLuc;
        private readonly string MCV = "PhuLucHopDongLaoDong";
        private bool? permission;
        public ActionResult Index(string id, string trangThai)
        {
            try
            {
                #region Role user
                permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion
                BindDataTrangThai(MCV);
                ViewBag.DSPhuLuc = context.sp_NS_PhuLucHopDong_Index(trangThai, id).ToList();
                ViewBag.SoHopDong = id;
                if (context.sp_NS_PhuLucHopDong_Index(trangThai, id).Where(d => d.maBuocDuyet == "DTN" || d.maBuocDuyet == "DD").Count() == context.sp_NS_PhuLucHopDong_Index(trangThai, id).Count())
                {
                    ViewBag.FlagPhuLuc = "true";
                }
                else
                {
                    ViewBag.FlagPhuLuc = "false";
                }
                return View();
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }

        public ActionResult ViewIndex(string id, string trangThai)
        {
            try
            {
                #region Role user
                permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion
                BindDataTrangThai(MCV);
                ViewBag.DSPhuLuc = context.sp_NS_PhuLucHopDong_Index(trangThai, id).ToList();
                ViewBag.SoHopDong = id;
                return PartialView("ViewIndex");
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }

        //
        // GET: /PhuLucHopDong/Details/5

        public ActionResult Details(string id)
        {
            try
            {
                #region Role user
                permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion
                var dsPhuLuc = context.tbl_NS_PhuLucHopDongs.Where(d => d.soPhuLuc == id).FirstOrDefault();

                model = new PhuLucHopDongModel();

                ThongTinPhuLucHopDong(id);
                var hinhThucLuongs = context.tbl_NS_HinhThucLuongs.ToList();
                hinhThucLuongs.Insert(0, new BatDongSan.Models.NhanSu.tbl_NS_HinhThucLuong { maHinhThuc = "", tenHinhThuc = "--Chọn--" });
                ViewBag.HinhThucLuongs = new SelectList(hinhThucLuongs, "maHinhThuc", "tenHinhThuc", model != null ? model.maHinhThuc : "");
                var dmThues = context.tbl_NS_DMThues.ToList();
                dmThues.Insert(0, new BatDongSan.Models.NhanSu.tbl_NS_DMThue { maThue = "", tenThue = "--Chọn--" });
                ViewBag.DMThues = new SelectList(dmThues, "maThue", "tenThue", model != null ? model.maThue : "");
                var loaiHopDongs = context.tbl_DM_LoaiHopDongLaoDongs.ToList();
                loaiHopDongs.Insert(0, new BatDongSan.Models.NhanSu.tbl_DM_LoaiHopDongLaoDong { maLoaiHopDong = "", tenLoaiHopDong = "--Chọn--" });
                ViewBag.LoaiHopDongs = new SelectList(loaiHopDongs, "maLoaiHopDong", "tenLoaiHopDong", model != null ? model.maLoaiHopDong : "");
                var baoHiems = context.tbl_NS_BaoHiems.ToList();
                baoHiems.Insert(0, new BatDongSan.Models.NhanSu.tbl_NS_BaoHiem { maBaoHiem = "", tenBaoHiem = "--Chọn--" });
                ViewBag.BaoHiems = new SelectList(baoHiems, "maBaoHiem", "tenBaoHiem", model != null ? model.maBaoHiem : "");
                //   TinhPhanBoMucLuong(model.mucLuongMoi, model.maNhanVien, false, id);
                return View(model);
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }

        //
        // GET: /PhuLucHopDong/Create

        public ActionResult Create(string id)
        {
            try
            {
                #region Role user
                permission = GetPermission(MCV, BangPhanQuyen.QuyenThem);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion
                context = new LinqNhanSuDataContext();
                hopDong = context.tbl_NS_HopDongLaoDongs.Where(s => s.soHopDong == id).FirstOrDefault();
                SetModelData();
                GetAllDropdownList();
                return View(model);
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }

        //
        // POST: /PhuLucHopDong/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                #region Role user
                permission = GetPermission(MCV, BangPhanQuyen.QuyenThem);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion
                // TODO: Add insert logic here
                phuLuc = new tbl_NS_PhuLucHopDong();
                BindDataToSave(collection, true);
                context.tbl_NS_PhuLucHopDongs.InsertOnSubmit(phuLuc);
                context.SubmitChanges();
                return RedirectToAction("Edit", new { id = phuLuc.soPhuLuc });
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }

        //
        // GET: /PhuLucHopDong/Edit/5

        public ActionResult Edit(string id)
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
                var dsPhuLuc = context.tbl_NS_PhuLucHopDongs.Where(d => d.soPhuLuc == id).FirstOrDefault();
                model = new PhuLucHopDongModel();
                ThongTinPhuLucHopDong(id);
                //TinhPhanBoMucLuong(model.mucLuongMoi, model.maNhanVien, false, id);
                GetAllDropdownList();
                return View(model);
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }

        //
        // POST: /PhuLucHopDong/Edit/5

        [HttpPost]
        public ActionResult Edit(FormCollection collection)
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
                // TODO: Add update logic here
                phuLuc = new tbl_NS_PhuLucHopDong();
                BindDataToSave(collection, false);
                context.SubmitChanges();
                return RedirectToAction("Edit", new { id = phuLuc.soPhuLuc });
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }

        // POST: /HopDongLaoDong/Delete/5

        [HttpPost]
        public ActionResult Delete(string id)
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

                LinqHeThongDataContext ht = new LinqHeThongDataContext();
                var delPhuLuc = context.tbl_NS_PhuLucHopDongs.Where(d => d.soPhuLuc == id);
                context.tbl_NS_PhuLucHopDongs.DeleteAllOnSubmit(delPhuLuc);
                string soHopDong = delPhuLuc.Select(d => d.soHopDong).FirstOrDefault() ?? string.Empty;
                var delChiTietPL = context.tbl_NS_PhuLucHopDongLaoDong_PhuCapNhanViens.Where(d => d.soPhuLuc == id);
                context.tbl_NS_PhuLucHopDongLaoDong_PhuCapNhanViens.DeleteAllOnSubmit(delChiTietPL);

                var nd = ht.tbl_HT_DMNguoiDuyets.Where(d => d.maPhieu == id);
                ht.tbl_HT_DMNguoiDuyets.DeleteAllOnSubmit(nd);

                context.SubmitChanges();
                ht.SubmitChanges();
                return RedirectToAction("Index", "PhuLucHopDong", new { id = soHopDong });
            }
            catch
            {
                return View();
            }
        }
        public int CreatePhuLucHopDong()
        {
            int count = 0;
            try
            {

                context = new LinqNhanSuDataContext();
                var lstHopDong = context.tbl_NS_HopDongLaoDongs.ToList();
                
                foreach (var item in lstHopDong)
                {
                    var checkMaNV = context.tbl_NS_NhanViens.Where(d => d.maNhanVien == item.maNhanVien && d.trangThai == 0 && d.maChiNhanhNganHang.Trim() =="MB").FirstOrDefault();
                    if (checkMaNV != null)
                    {
                       
                        hopDong = new tbl_NS_HopDongLaoDong();
                        hopDong.soHopDong = item.soHopDong;
                        SetModelDataPLHD();
                        if (model.duocPhepTao == 0)
                        {
                            phuLuc = new tbl_NS_PhuLucHopDong();
                            phuLuc.soPhuLuc = model.soPhuLuc;
                            phuLuc.ngayLap = DateTime.Now;
                            phuLuc.nguoiLap = GetUser().manv;
                            phuLuc.soHopDong = item.soHopDong;
                            phuLuc.noiDungThayDoi = "Admin tạo phụ lục tự động.";
                            phuLuc.ghiChu = string.Empty;
                            phuLuc.maLoaiHopDong = model.maLoaiHopDong;
                            phuLuc.ngayHieuLuc = model.ngayHieuLuc;
                            phuLuc.giaHanDen = model.giaHanDen;
                            phuLuc.tongLuong = model.tongLuong;
                            phuLuc.doanPhi = (decimal?)model.doanPhi;
                            phuLuc.dangPhi = (decimal?)model.dangPhi;
                            phuLuc.tienDienThoai = model.tienDienThoai;
                            phuLuc.tienAnGiuaCa = model.tienAnGiuaCa;
                            phuLuc.checkPhuCapDiLai = model.checkPhuCapDiLai;
                            phuLuc.luongDongBaoHiem = model.luongDongBH;
                            phuLuc.khoanBoSungLuong = model.khoanBoSungLuong;
                            phuLuc.luong = model.luong;
                            phuLuc.phuCapLuong = (decimal?)model.phuCapLuong;
                            phuLuc.bac = model.bac;
                            phuLuc.bacChucVu = model.bacChucVu;
                            phuLuc.luongCoBan = model.luongCoBan;
                            phuLuc.luongThanhTich = model.luongThanhTich;
                            phuLuc.hinhThucLuong = model.maHinhThuc;
                            phuLuc.baoHiem = model.maBaoHiem;
                            phuLuc.thue = model.thue;
                            phuLuc.baoHiem = model.baoHien;
                            phuLuc.hinhThucLuong = model.hinhThucLuong;
                            if (phuLuc.checkPhuCapDiLai == true)
                            {
                                phuLuc.phuCapDiLaiNew = 200000;
                            }
                            else
                            {
                                phuLuc.phuCapDiLaiNew = 0;
                            }
                            phuLuc.ngayHieuLuc = Convert.ToDateTime("2018-01-01");
                            phuLuc.maLoaiHopDong = model.maLoaiHopDongPL;
                            context.tbl_NS_PhuLucHopDongs.InsertOnSubmit(phuLuc);
                            context.SubmitChanges();
                            tbl_NS_LichSuTaoPhuLuc tblTaoPhuLuc = new tbl_NS_LichSuTaoPhuLuc();
                            tblTaoPhuLuc.maNV = hopDong.maNhanVien;
                            tblTaoPhuLuc.maPhuLuc = phuLuc.soPhuLuc;
                            tblTaoPhuLuc.maPhuLucCu = model.maPhuLucCu;
                            context.tbl_NS_LichSuTaoPhuLucs.InsertOnSubmit(tblTaoPhuLuc);
                            context.SubmitChanges();
                            count++;
                        }
                    }
                }
            }
            catch(Exception ex){
                return 0;
            }
            return count;
        }
        public void SetModelDataPLHD()
        {
            //Kiểm tra hợp đồng đó có phụ lục hay chưa nếu có thì lấy phụ lục mới nhất và phụ lục đó đã duyệt
            DMNguoiDuyetController nd = new DMNguoiDuyetController();
            string soPhuLuc = string.Empty;
            var dsPhuLuc = context.tbl_NS_PhuLucHopDongs.Where(d => d.soHopDong == hopDong.soHopDong).Select(d => new PhuLucHopDongModel
            {
                soPhuLuc = d.soPhuLuc,
                Duyet = nd.GetDetailByMaPhieuTheoQuiTrinhDong(d.soPhuLuc, d.maQuiTrinhDuyet ?? 0),
            }).ToList();
            if (dsPhuLuc != null && dsPhuLuc.Count() > 0)
            {
                soPhuLuc = dsPhuLuc.OrderByDescending(d => d.soPhuLuc).Where(d => d.Duyet.maBuocDuyet == "DTN" || d.Duyet.maBuocDuyet == "DD").Select(d => d.soPhuLuc).FirstOrDefault() ?? string.Empty;
            }
            if (soPhuLuc != "")
            {
                model = (from pl in context.tbl_NS_PhuLucHopDongs
                         join hd in context.tbl_NS_HopDongLaoDongs on pl.soHopDong equals hd.soHopDong
                         join nv in context.tbl_NS_NhanViens on hd.maNhanVien equals nv.maNhanVien
                         where pl.soPhuLuc == soPhuLuc
                         select new PhuLucHopDongModel
                         {
                             maNhanVien = hd.maNhanVien,
                             tenNhanVien = nv.ho + " " + nv.ten,
                             soHopDong = hd.soHopDong,
                             maLoaiHopDong = hd.maLoaiHopDong,
                             ngayHieuLuc = pl.ngayHieuLuc,
                             giaHanDen = pl.giaHanDen,
                             nguoiCapNhat = (string)Session["TenNhanVien"],
                             doanPhi = (double?)pl.doanPhi ?? 0,
                             dangPhi = (double?)pl.dangPhi ?? 0,
                             tienDienThoai = pl.tienDienThoai ?? 0,

                             tienAnGiuaCa = pl.tienAnGiuaCa ?? false,
                             checkPhuCapDiLai = pl.checkPhuCapDiLai ?? false,
                             luongDongBH = (decimal?)pl.luongDongBaoHiem ?? 0,
                             khoanBoSungLuong = (decimal?)pl.khoanBoSungLuong ?? 0,
                             luong = pl.luong ?? 0,
                             tongLuong = pl.tongLuong ?? 0,
                             phuCapLuong = (double?)pl.phuCapLuong ?? 0,
                             luongCoBan = pl.luongCoBan,
                             luongThanhTich = pl.luongThanhTich,
                             bac = pl.bac,
                             bacChucVu = pl.bacChucVu,
                             phuCapDiLaiNew = (decimal?)pl.phuCapDiLaiNew ?? 0,
                             duocPhepTao = 0,
                             maPhuLucCu = pl.soPhuLuc,
                             thue = pl.thue,
                             baoHien = pl.baoHiem,
                             hinhThucLuong = pl.hinhThucLuong,
                             maLoaiHopDongPL = pl.maLoaiHopDong,
                         }).FirstOrDefault();
            }
            else {
                model.duocPhepTao = 1;
            }
            var nhanVien = context.sp_NS_NhanVien_Index(null, hopDong.maNhanVien, null, null, null).FirstOrDefault();
            model.tenChucDanh = nhanVien.TenChucDanh;
            model.soPhuLuc = IdGeneratorPLHD(hopDong.soHopDong);
        }

        public string IdGeneratorPLHD(string soHopDong)
        {
            StringBuilder sb = new StringBuilder();
            var date = DateTime.Now;
            int soLanTaoPhuLuc = context.tbl_NS_PhuLucHopDongs.Where(s => s.soHopDong == soHopDong).Count() + 1;
            return soHopDong + "-PLHD-" + soLanTaoPhuLuc.ToString();
        }
        public void SetModelData()
        {
            //Kiểm tra hợp đồng đó có phụ lục hay chưa nếu có thì lấy phụ lục mới nhất và phụ lục đó đã duyệt
            DMNguoiDuyetController nd = new DMNguoiDuyetController();
            string soPhuLuc = string.Empty;
            var dsPhuLuc = context.tbl_NS_PhuLucHopDongs.Where(d => d.soHopDong == hopDong.soHopDong).Select(d => new PhuLucHopDongModel
                {
                    soPhuLuc = d.soPhuLuc,
                    Duyet = nd.GetDetailByMaPhieuTheoQuiTrinhDong(d.soPhuLuc, d.maQuiTrinhDuyet ?? 0),
                }).ToList();
            if (dsPhuLuc != null && dsPhuLuc.Count() > 0)
            {
                soPhuLuc = dsPhuLuc.OrderByDescending(d => d.soPhuLuc).Where(d => d.Duyet.maBuocDuyet == "DTN" || d.Duyet.maBuocDuyet == "DD").Select(d => d.soPhuLuc).FirstOrDefault() ?? string.Empty;
            }
            if (soPhuLuc != "")
            {
                model = (from pl in context.tbl_NS_PhuLucHopDongs
                         join hd in context.tbl_NS_HopDongLaoDongs on pl.soHopDong equals hd.soHopDong
                         join nv in context.tbl_NS_NhanViens on hd.maNhanVien equals nv.maNhanVien
                         where pl.soPhuLuc == soPhuLuc
                         select new PhuLucHopDongModel
                         {
                             maNhanVien = hd.maNhanVien,
                             tenNhanVien = nv.ho + " " + nv.ten,
                             soHopDong = hd.soHopDong,
                             maLoaiHopDong = pl.maLoaiHopDong,
                             ngayHieuLuc = pl.ngayHieuLuc,
                             giaHanDen = pl.giaHanDen,
                             nguoiCapNhat = (string)Session["TenNhanVien"],
                             doanPhi = (double?)pl.doanPhi ?? 0,
                             dangPhi = (double?)pl.dangPhi ?? 0,
                             tienDienThoai = pl.tienDienThoai ?? 0,

                             tienAnGiuaCa = pl.tienAnGiuaCa ?? false,
                              checkPhuCapDiLai = pl.checkPhuCapDiLai ?? false,
                             luongDongBH = (decimal?)pl.luongDongBaoHiem ?? 0,
                             khoanBoSungLuong = (decimal?)pl.khoanBoSungLuong ?? 0,
                             luong = pl.luong ?? 0,
                             tongLuong = pl.tongLuong ?? 0,
                             phuCapLuong = (double?)pl.phuCapLuong ?? 0,
                             luongCoBan = pl.luongCoBan,
                             luongThanhTich = pl.luongThanhTich,
                             bac = pl.bac,
                             bacChucVu = pl.bacChucVu,
                             phuCapDiLaiNew = (decimal?)pl.phuCapDiLaiNew??0,

                             maHinhThuc =pl.hinhThucLuong,
                             maBaoHiem = pl.baoHiem,
                             maThue = pl.thue,
                             ngayBatDauTinhPhep = pl.ngayBatDauTinhPhep,

                         }).FirstOrDefault();
            }
            else
            {
                model = (from hd in context.tbl_NS_HopDongLaoDongs
                         join lhd in context.tbl_DM_LoaiHopDongLaoDongs on hd.maLoaiHopDong equals lhd.maLoaiHopDong
                         join th in context.tbl_DM_ThoiHanHopDongLaoDongs on hd.idThoiHanHopDong equals th.id
                         join nv in context.tbl_NS_NhanViens on hd.maNhanVien equals nv.maNhanVien
                         where hd.soHopDong == hopDong.soHopDong
                         select new PhuLucHopDongModel
                         {
                             maNhanVien = hd.maNhanVien,
                             tenNhanVien = nv.ho + " " + nv.ten,
                             mucDieuChinh = 0,
                             soHopDong = hd.soHopDong,
                             maLoaiHopDong = hd.maLoaiHopDong,
                             ngayHieuLuc = DateTime.Now,
                             giaHanDen = DateTime.Now,
                             nguoiCapNhat = (string)Session["TenNhanVien"],
                             doanPhi = (double?)hd.doanPhi ?? 0,
                             dangPhi = (double?)hd.dangPhi ?? 0,
                             tienDienThoai = hd.tienDienThoai ?? 0,
                             tienAnGiuaCa = hd.tienAnGiuaCa ?? false,
                             checkPhuCapDiLai = hd.checkPhuCapDiLai ?? false,
                             luongDongBH = hd.luongDongBaoHiem ?? 0,
                             khoanBoSungLuong = hd.khoanBoSungLuong ?? 0,
                             phuCapLuong = (double?)hd.phuCapLuong ?? 0,
                             luong = hd.luongThoaThuan ?? 0,
                             tongLuong = hd.tongLuong ?? 0,
                             luongCoBan = hd.luongCoBan,
                             luongThanhTich = hd.luongThanhTich,
                             bac = hd.bac,
                             bacChucVu = hd.bacChucVu,
                             phuCapDiLaiNew = (decimal?)hd.phuCapDiLaiNew ?? 200,

                             maHinhThuc = hd.hinhThucLuong,
                             maBaoHiem = hd.baoHiem,
                             maThue = hd.thue,
                             ngayBatDauTinhPhep = hd.ngayBatDauTinhPhep,
                         }).FirstOrDefault();
            }
            var nhanVien = context.sp_NS_NhanVien_Index(null, hopDong.maNhanVien, null, null, null).FirstOrDefault();
            model.tenChucDanh = nhanVien.TenChucDanh;
            model.soPhuLuc = IdGenerator(hopDong.soHopDong);
        }

        public string IdGenerator(string soHopDong)
        {
            StringBuilder sb = new StringBuilder();
            var date = DateTime.Now;
            int soLanTaoPhuLuc = context.tbl_NS_PhuLucHopDongs.Where(s => s.soHopDong == soHopDong).Count() + 1;
            return soHopDong + "-PLHD-" + soLanTaoPhuLuc.ToString();
        }



        public void GetAllDropdownList()
        {
            var hinhThucLuongs = context.tbl_NS_HinhThucLuongs.ToList();
            hinhThucLuongs.Insert(0, new BatDongSan.Models.NhanSu.tbl_NS_HinhThucLuong { maHinhThuc = "", tenHinhThuc = "--Chọn--" });
            ViewBag.HinhThucLuongs = new SelectList(hinhThucLuongs, "maHinhThuc", "tenHinhThuc", model != null ? model.maHinhThuc : "");
            var dmThues = context.tbl_NS_DMThues.ToList();
            dmThues.Insert(0, new BatDongSan.Models.NhanSu.tbl_NS_DMThue { maThue = "", tenThue = "--Chọn--" });
            ViewBag.DMThues = new SelectList(dmThues, "maThue", "tenThue", model != null ? model.maThue : "");

            var baoHiems = context.tbl_NS_BaoHiems.ToList();
            baoHiems.Insert(0, new BatDongSan.Models.NhanSu.tbl_NS_BaoHiem { maBaoHiem = "", tenBaoHiem = "--Chọn--" });
            ViewBag.BaoHiems = new SelectList(baoHiems, "maBaoHiem", "tenBaoHiem", model != null ? model.maBaoHiem : "");
            var loaiHopDongs = context.tbl_DM_LoaiHopDongLaoDongs.ToList();
            loaiHopDongs.Insert(0, new BatDongSan.Models.NhanSu.tbl_DM_LoaiHopDongLaoDong { maLoaiHopDong = "", tenLoaiHopDong = "--Chọn--" });
            ViewBag.LoaiHopDongs = new SelectList(loaiHopDongs, "maLoaiHopDong", "tenLoaiHopDong", model != null ? model.maLoaiHopDong : "");
            var bacs = (from b in context.GetTable<tbl_DM_ThangLuongCongTy>()
                        select new
                        {
                            Key = b.bac,
                            Value = b.bac
                        }).ToList().Distinct();
            ViewBag.Bacs = new SelectList(bacs, "Key", "Value", model.bac);

            model.bac = model.bac ?? bacs.FirstOrDefault().Key;
            var bacChucVus = (from b in context.GetTable<tbl_DM_ThangLuongCongTy>()
                              where b.bac == model.bac
                              select new
                              {
                                  Key = b.capBacChucVu,
                                  Value = b.capBacChucVu
                              }).ToList();

            ViewBag.BacChucVus = new SelectList(bacChucVus, "Key", "Value", model.bacChucVu);
        }

        public ActionResult GetBacChucVu(int id)
        {
            try
            {
                var bacChucVus = context.GetTable<tbl_DM_ThangLuongCongTy>().Where(s => s.bac == id).ToList();
                return Json(bacChucVus);
            }
            catch
            {
                return View();
            }
        }

        public ActionResult GetThangLuong(int bac, int bacChucVu)
        {
            try
            {
                var thangLuongs = context.GetTable<tbl_DM_ThangLuongCongTy>().Where(s => s.bac == bac && s.capBacChucVu == bacChucVu).ToList();
                model = new PhuLucHopDongModel();
                model.thangLuongs = thangLuongs;
                return PartialView("PartialThangLuong", model);
            }
            catch
            {
                return View();
            }
        }


        #region Insert, update details phụ lục hợp đồng lao động

        public void BindDataToSave(FormCollection col, bool isCreate)
        {
            var dsHopDong = context.tbl_NS_HopDongLaoDongs.Where(d => d.soHopDong == col.Get("soHopDong")).FirstOrDefault();
            if (isCreate == true)
            {
                phuLuc.soPhuLuc = IdGenerator(dsHopDong.soHopDong);
                phuLuc.ngayLap = DateTime.Now;
                phuLuc.nguoiLap = GetUser().manv;
            }
            else
            {
                phuLuc = context.tbl_NS_PhuLucHopDongs.Where(d => d.soPhuLuc == col.Get("soPhuLuc")).FirstOrDefault();
            }
            phuLuc.soHopDong = col.Get("soHopDong");
            phuLuc.noiDungThayDoi = col.Get("noiDungThayDoi");
            phuLuc.ghiChu = col.Get("ghiChu");
            phuLuc.maLoaiHopDong = col.Get("maLoaiHopDong");
            phuLuc.ngayHieuLuc = String.IsNullOrEmpty(col["ngayHieuLuc"]) ? (DateTime?)null : DateTime.ParseExact(col["ngayHieuLuc"], "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            phuLuc.ngayBatDauTinhPhep = String.IsNullOrEmpty(col["ngayBatDauTinhPhep"]) ? (DateTime?)null : DateTime.ParseExact(col["ngayBatDauTinhPhep"], "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            phuLuc.giaHanDen = String.IsNullOrEmpty(col["giaHanDen"]) ? (DateTime?)null : DateTime.ParseExact(col["giaHanDen"], "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            phuLuc.tongLuong = Convert.ToDecimal(col.Get("tongLuong"));
            phuLuc.doanPhi = string.IsNullOrEmpty(col["doanPhi"]) ? 0 : Convert.ToDecimal(col["doanPhi"].Replace('%', '0').Replace(' ', '0'));
            phuLuc.dangPhi = string.IsNullOrEmpty(col["dangPhi"]) ? 0 : Convert.ToDecimal(col["dangPhi"].Replace('%', '0').Replace(' ', '0'));
            phuLuc.tienDienThoai = string.IsNullOrEmpty(col["tienDienThoai"]) ? 0 : Convert.ToDecimal(col["tienDienThoai"]);
            phuLuc.tienAnGiuaCa = col["tienAnGiuaCa"].Contains("true");
            phuLuc.checkPhuCapDiLai = col["checkPhuCapDiLai"].Contains("true");
            phuLuc.luongDongBaoHiem = string.IsNullOrEmpty(col["luongDongBH"]) ? 0 : Convert.ToDecimal(col["luongDongBH"]);
            phuLuc.khoanBoSungLuong = string.IsNullOrEmpty(col["khoanBoSungLuong"]) ? 0 : Convert.ToDecimal(col["khoanBoSungLuong"]);
            phuLuc.luong = string.IsNullOrEmpty(col["luong"]) ? 0 : Convert.ToDecimal(col["luong"]);
            phuLuc.phuCapLuong = string.IsNullOrEmpty(col["phuCapLuong"]) ? 0 : Convert.ToDecimal(col["phuCapLuong"]);
            phuLuc.bac = Convert.ToInt32(col["bac"]);
            phuLuc.bacChucVu = Convert.ToInt32(col["bacChucVu"]);
            phuLuc.luongCoBan = string.IsNullOrEmpty(col["luongCoBan"]) ? 0 : Convert.ToDecimal(col["luongCoBan"]);
            phuLuc.luongThanhTich = string.IsNullOrEmpty(col["luongThanhTich"]) ? 0 : Convert.ToDecimal(col["luongThanhTich"]);
            phuLuc.hinhThucLuong = col["maHinhThuc"];
            phuLuc.baoHiem = col["maBaoHiem"];
            phuLuc.thue = col["maThue"];
            if (phuLuc.checkPhuCapDiLai == true)
            {
                phuLuc.phuCapDiLaiNew = 200000;
            }
            else {
                phuLuc.phuCapDiLaiNew = 0;
            }
            }

        public void ThongTinPhuLucHopDong(string soPhuLuc)
        {
            DMNguoiDuyetController nd = new DMNguoiDuyetController();
            model = context.tbl_NS_PhuLucHopDongs.Where(d => d.soPhuLuc == soPhuLuc).Select(d => new PhuLucHopDongModel
                {
                    nguoiLap = d.nguoiLap,
                    soPhuLuc = d.soPhuLuc,
                    soHopDong = d.soHopDong,
                    ngayHieuLuc = d.ngayHieuLuc,
                    giaHanDen = d.giaHanDen,
                    nguoiCapNhat = ((context.tbl_NS_NhanViens.Where(c => c.maNhanVien == d.nguoiLap).Select(c => c.ho).FirstOrDefault() ?? string.Empty) + " " + (context.tbl_NS_NhanViens.Where(c => c.maNhanVien == d.nguoiLap).Select(c => c.ho).FirstOrDefault() ?? string.Empty)),
                    noiDungThayDoi = d.noiDungThayDoi,
                    ghiChu = d.ghiChu,
                    maQuiTrinhDuyet = d.maQuiTrinhDuyet ?? 0,
                    Duyet = nd.GetDetailByMaPhieuTheoQuiTrinhDong(d.soPhuLuc, d.maQuiTrinhDuyet ?? 0),
                    doanPhi = (double?)d.doanPhi ?? 0,
                    dangPhi = (double?)d.dangPhi ?? 0,
                    tienDienThoai = d.tienDienThoai ?? 0,
                    tienAnGiuaCa = d.tienAnGiuaCa ?? false,
                    checkPhuCapDiLai = d.checkPhuCapDiLai??false,
                    luongDongBH = (decimal?)d.luongDongBaoHiem ?? 0,
                    khoanBoSungLuong = (decimal?)d.khoanBoSungLuong ?? 0,
                    luong = d.luong ?? 0,
                    tongLuong = d.tongLuong ?? 0,
                    phuCapLuong = (double?)d.phuCapLuong ?? 0,
                    luongCoBan = d.luongCoBan,
                    luongThanhTich = d.luongThanhTich,
                    bac = d.bac,
                    bacChucVu = d.bacChucVu,
                    maHinhThuc = d.hinhThucLuong ?? null,
                    maBaoHiem = d.baoHiem ?? null,
                    maThue = d.thue ?? null,
                    maLoaiHopDong = d.maLoaiHopDong ?? null,
                    phuCapDiLaiNew = (decimal?)d.phuCapDiLaiNew??0,
                    ngayBatDauTinhPhep = d.ngayBatDauTinhPhep,
                }).FirstOrDefault();
            model.maNhanVien = context.tbl_NS_HopDongLaoDongs.Where(c => c.soHopDong == model.soHopDong).Select(c => c.maNhanVien).FirstOrDefault() ?? string.Empty;
            var nhanVien = context.sp_NS_NhanVien_Index(null, model.maNhanVien, null, null, null).FirstOrDefault();
            if (nhanVien != null)
            {
                model.tenChucDanh = nhanVien.TenChucDanh;
                model.tenNhanVien = nhanVien.hoVaTen;
            }
            model.soPhuLuc = soPhuLuc;
            //model.tenNhanVien = context.sp_NS_NhanVien_Index(null, model.maNhanVien, null, null, null).Select(c => c.hoVaTen).FirstOrDefault() ?? string.Empty;

            LinqHeThongDataContext ht = new LinqHeThongDataContext();
            string hoTen = (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ho).FirstOrDefault() ?? string.Empty) + " " + (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ten).FirstOrDefault() ?? string.Empty);
            ViewBag.HoTen = hoTen;
            int trangThaiHT = (int?)ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == model.soPhuLuc).Select(d => d.trangThai).FirstOrDefault() ?? 0;
            ViewBag.TenTrangThaiDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == trangThaiHT).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
            ViewBag.URL = Request.Url.AbsoluteUri.ToString();
            var maBuocDuyet = ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == model.soPhuLuc).FirstOrDefault();
            ViewBag.TenBuocDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == (maBuocDuyet == null ? 0 : maBuocDuyet.trangThai)).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
        }

        #endregion

        #region Tính phân bổ mức lương của phụ lục
        /// <summary>
        /// Tính phân bổ mức lương
        /// </summary>
        /// <param name="luongThoaThuan"></param>
        /// <param name="maNhanVien"></param>
        /// <returns></returns>
        public ActionResult PhanBoLuong(decimal? luongThoaThuan, string maNhanVien)
        {
            TinhPhanBoMucLuong(luongThoaThuan, maNhanVien, true, string.Empty);
            if (phanBoLuong.flag == 0)
            {
                return Json(new { mucLuong = "Chưa có dữ liệu trong Bảng Phân Tách Lương cho mức lương này" }, JsonRequestBehavior.AllowGet);
            }
            return PartialView("PartialPhanBoLuong", phanBoLuong);

        }
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ViewsApproval(string id)
        {
            try
            {
                return RedirectToAction("Details", new { id = id });// detail de duyet
            }
            catch
            {
                return View();
            }
        }
        public void TinhPhanBoMucLuong(decimal? luongThoaThuan, string maNhanVien, bool phanBo, string soPhuLuc)
        {
            phanBoLuong = new PhanBoLuongModel();
            phanBoLuong.flag = 1;
            var nhanVien = context.tbl_NS_NhanViens.Where(s => s.maNhanVien == maNhanVien).FirstOrDefault();
            phanBoLuong.tenNhanVien = nhanVien.ho + " " + nhanVien.ten;
            phanBoLuong.maNhanVien = nhanVien.maNhanVien;
            var mucLuong = context.tbl_NS_BangPhanTachMucLuongs.Where(s => s.mucLuongTu <= luongThoaThuan && s.mucLuongDen >= luongThoaThuan).FirstOrDefault();
            if (mucLuong == null)
            {
                phanBoLuong.flag = 0;
            }
            else
            {
                phanBoLuong.luongCoBan = mucLuong.luongCoBan;
                phanBoLuong.mucTinhPhanBo = mucLuong.tenMucLuong;
                phanBoLuong.mucLuongThoaThuan = luongThoaThuan;
                phanBoLuong.ngayApDung = DateTime.Now;
                phanBoLuong.tongPhuCap = luongThoaThuan - mucLuong.luongCoBan;
                //if (phanBo == true)
                //{
                phanBoLuong.chiTiets = (from ct in context.tbl_NS_BangPhanTachMucLuongChiTiets
                                        join b in context.tbl_NS_BangPhanTachMucLuongs on ct.maMucLuong equals b.maMucLuong
                                        join pc in context.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhuCap>() on ct.maPhuCap equals pc.maPhuCap
                                        join l in context.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_LoaiPhuCap>() on pc.loaiPhuCap equals l.id
                                        where ct.maMucLuong == mucLuong.maMucLuong
                                        select new BangPhanTachLuongChiTietModel
                                        {
                                            ghiChu = ct.ghiChu,
                                            id = ct.id,
                                            loaiTyLe = ct.loaiTyLe,
                                            idLoaiPhuCap = pc.loaiPhuCap,
                                            tenLoaiPhuCap = l.tenLoaiPhuCap,
                                            maMucLuong = ct.maMucLuong,
                                            maPhuCap = ct.maPhuCap,
                                            salaryTemplate = ct.salaryTemplate,
                                            tenPhuCap = pc.tenPhuCap,
                                            tenMucLuong = b.tenMucLuong,
                                            tyLe = ct.tyLe
                                        }).ToList();
                //}
                //else
                //{
                //    phanBoLuong.chiTiets = (from plct in context.tbl_NS_PhuLucHopDongLaoDong_PhuCapNhanViens
                //                            join ct in context.tbl_NS_BangPhanTachMucLuongChiTiets on plct.maPhuCap equals ct.maPhuCap
                //                            join b in context.tbl_NS_BangPhanTachMucLuongs on ct.maMucLuong equals b.maMucLuong
                //                            join pc in context.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhuCap>() on ct.maPhuCap equals pc.maPhuCap
                //                            join l in context.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_LoaiPhuCap>() on pc.loaiPhuCap equals l.id
                //                            where ct.maMucLuong == mucLuong.maMucLuong && plct.soPhuLuc == soPhuLuc && plct.loaiTyLe == ct.loaiTyLe
                //                            select new BangPhanTachLuongChiTietModel
                //                            {
                //                                ghiChu = plct.ghiChu,
                //                                id = ct.id,
                //                                loaiTyLe = ct.loaiTyLe,
                //                                idLoaiPhuCap = pc.loaiPhuCap,
                //                                tenLoaiPhuCap = l.tenLoaiPhuCap,
                //                                maMucLuong = ct.maMucLuong,
                //                                maPhuCap = ct.maPhuCap,
                //                                salaryTemplate = plct.soTien.ToString(),
                //                                tenPhuCap = pc.tenPhuCap,
                //                                tenMucLuong = b.tenMucLuong,
                //                                tyLe = ct.tyLe
                //                            }).ToList();
                //}

                phanBoLuong.tongPhuCapKhongTheoTyLe = phanBoLuong.chiTiets.Where(s => s.loaiTyLe == "I").Select(s => Convert.ToDecimal(s.salaryTemplate)).Sum();
                phanBoLuong.tongPhuCapTheoTyLe = phanBoLuong.tongPhuCap - phanBoLuong.tongPhuCapKhongTheoTyLe;
                foreach (var item in phanBoLuong.chiTiets.Where(s => s.loaiTyLe == "II"))
                {
                    item.salaryTemplate = (item.tyLe * phanBoLuong.tongPhuCapTheoTyLe / 100).ToString();
                }

                phanBoLuong.tongTyle = phanBoLuong.chiTiets.Select(s => s.tyLe).Sum();
                ViewBag.PhanBoLuong = phanBoLuong;
            }

        }
        #endregion
    }
}
