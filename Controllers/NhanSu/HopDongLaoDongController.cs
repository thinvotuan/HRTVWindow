using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Helper.Common;
using BatDongSan.Helper.Utils;
using BatDongSan.Models.DanhMuc;
using BatDongSan.Models.HeThong;
using BatDongSan.Models.NhanSu;
using BatDongSan.Utils.Paging;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.UserModel.Contrib;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using Worldsoft.Mvc.Web.Util;

namespace BatDongSan.Controllers.NhanSu
{
    public class HopDongLaoDongController : ApplicationController
    {
        LinqHeThongDataContext lqHT = new LinqHeThongDataContext();
        private LinqDanhMucDataContext linqDanhMuc = new LinqDanhMucDataContext();
        private LinqNhanSuDataContext context = new LinqNhanSuDataContext();
        private IList<sp_NS_HopDongLaoDong_IndexResult> hopDongs;
        private IList<tbl_NS_PhuCapNhanVien> phuCaps;
        private PhanBoLuongModel phanBoLuong;
        private HopDongLaoDongModel model;
        private tbl_NS_HopDongLaoDong hopDong;
        LinqDanhMucDataContext linqDM = new LinqDanhMucDataContext();
        private IList<tbl_DM_PhongBan> phongBans;
        private StringBuilder buildTree = null;
        private readonly string MCV = "HopDongLaoDong";
        private bool? permission;

        public ActionResult Index(int? page, int? pageSize, string loaiHopDong, string searchString, string maPhongBan, string trangThai, int? trangThaiNV)
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

                // TaoPhuLucTuDong();
                BindDataTrangThai(MCV);
                buildTree = new StringBuilder();
                phongBans = linqDM.tbl_DM_PhongBans.ToList();
                buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
                ViewBag.PhongBans = buildTree.ToString();
                //
                int currentPageIndex = page.HasValue ? page.Value : 1;
                pageSize = pageSize ?? 30;

                var loaiHopDongs = context.tbl_DM_LoaiHopDongLaoDongs.ToList();
                loaiHopDongs.Insert(0, new BatDongSan.Models.NhanSu.tbl_DM_LoaiHopDongLaoDong { maLoaiHopDong = "", tenLoaiHopDong = "[Tất cả loại hợp đồng]" });
                ViewBag.LoaiHopDongs = new SelectList(loaiHopDongs, "maLoaiHopDong", "tenLoaiHopDong");
                hopDongs = context.sp_NS_HopDongLaoDong_Index(null, loaiHopDong, maPhongBan, searchString, trangThaiNV, currentPageIndex, 30, trangThai).ToList();

                int? tongSoDong = 0;
                try
                {
                    ViewBag.Count = hopDongs[0].tongSoDong;
                    tongSoDong = hopDongs[0].tongSoDong;
                }
                catch
                {
                    ViewBag.Count = 0;
                }
                ViewBag.trangThai = trangThai;
                ViewBag.searchString = searchString;
                ViewBag.loaiHopDong = loaiHopDong;
                ViewBag.maPhongBan = maPhongBan;
                ViewBag.trangThaiNV = trangThaiNV;
                if (Request.IsAjaxRequest())
                {
                    return PartialView("PartialIndex", hopDongs.ToPagedList(currentPageIndex, 30, true, tongSoDong));
                }


                return View(hopDongs.ToPagedList(currentPageIndex, 30, true, tongSoDong));
            }
            catch
            {
                return View("Error");
            }
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
                return View("Error");
            }
        }
        //
        // GET: /HopDongLaoDong/Details/5

        public ActionResult Details(string id)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXemChiTiet);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                hopDong = context.tbl_NS_HopDongLaoDongs.Where(s => s.soHopDong == id).FirstOrDefault();
                SetModelData();
                GetAllDropdownList();
                //TinhPhanBoMucLuong(model.luongThoaThuan, model.maNhanVien, hopDong.soHopDong);
                return View(model);
            }
            catch
            {
                return View("Error");
            }
        }

        //
        // GET: /HopDongLaoDong/Create

        public ActionResult Create()
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
                context = new LinqNhanSuDataContext();
                model = new HopDongLaoDongModel();
                model.soHopDong = IdGeneratorHDLD();
                model.ngayBatDau = DateTime.Now;
                model.ngayKy = DateTime.Now;
                model.ngayBatDau = DateTime.Now;
                model.ngayBatDauTinhPhep = DateTime.Now;

                //model.ngayKetThuc = DateTime.Now;
                model.luongThoaThuan = 0;
                model.tongLuong = 0;
                model.tyLeHuong = 100;
                model.luongThanhTich = 0;
                model.tienXang = 0;
                model.tienDiLai = 0;
                model.luongThanhTich = 0;
                model.luongCoBan = 0;
                model.phuCapDiLaiNew = 200000;
                model.thangLuongs = linqDM.tbl_DM_ThangLuongCongTies.ToList();


                GetAllDropdownList();
                return View(model);
            }
            catch
            {
                return View("Error");
            }
        }

        //
        // POST: /HopDongLaoDong/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
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
                hopDong = new tbl_NS_HopDongLaoDong();
                GetDataFromView(collection, true);
                hopDong.soHopDong = IdGeneratorHDLD();
                context.tbl_NS_HopDongLaoDongs.InsertOnSubmit(hopDong);
                // SavePhuCapLuong(collection);
                context.SubmitChanges();
                SaveActiveHistory("Thêm hợp đồng lao động: " + hopDong.soHopDong);
                return RedirectToAction("Edit", new { id = hopDong.soHopDong });
            }
            catch
            {
                return View("Error");
            }
        }

        //
        // GET: /HopDongLaoDong/Edit/5

        public ActionResult Edit(string id)
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
                hopDong = context.tbl_NS_HopDongLaoDongs.Where(s => s.soHopDong == id).FirstOrDefault();

                SetModelData();
                GetAllDropdownList();
                // TinhPhanBoMucLuong(model.luongThoaThuan, model.maNhanVien, hopDong.soHopDong);
                return View(model);
            }
            catch
            {
                return View("Error");
            }
        }

        //
        // POST: /HopDongLaoDong/Edit/5

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
                hopDong = context.tbl_NS_HopDongLaoDongs.Where(s => s.soHopDong == id).FirstOrDefault();
                GetDataFromView(collection, false);
                // SavePhuCapLuong(collection);
                context.SubmitChanges();
                SaveActiveHistory("Sửa hợp đồng lao động: " + hopDong.soHopDong);
                return RedirectToAction("Edit", new { id = hopDong.soHopDong });
            }
            catch
            {
                return View("Error");
            }
        }
        public string IdGeneratorHDLD()
        {

            StringBuilder sb = new StringBuilder();
            var date = DateTime.Now;
            string lastID = context.tbl_NS_HopDongLaoDongs.OrderByDescending(d => d.soHopDong).Select(d => d.soHopDong).FirstOrDefault() ?? string.Empty;

            if (String.IsNullOrEmpty(lastID))
            {
                return "HDLD-" + "0001";
            }
            else
            {
                int? idSuffix = Convert.ToInt32(lastID.Substring(lastID.Length - 4)) + 1;
                if (idSuffix <= 0 || idSuffix == null)
                    return "HDLD-" + "0001";
                else
                {
                    sb.Append(idSuffix.ToString());
                    while (sb.Length < 4)
                    {
                        sb.Insert(0, "0");
                    }
                    return "HDLD-" + sb.ToString();
                }
            }
        }

        public void GetDataFromView(FormCollection collection, bool isCreate)
        {

            hopDong.soHopDong = collection["soHopDong"];

            hopDong.ghiChu = collection["ghiChuMain"];
            hopDong.idThoiHanHopDong = Convert.ToByte(collection["idThoiHanHopDong"]);
            hopDong.maChucDanh = collection["maChucDanh"];
            hopDong.maLoaiHopDong = collection["maLoaiHopDong"];
            hopDong.maNhanVien = collection["maNhanVien"];
            //hopDong.luongHopDong = Convert.ToDecimal(collection["luongHopDong"]);
            hopDong.ngayBatDau = String.IsNullOrEmpty(collection["ngayBatDau"]) ? (DateTime?)null : DateTime.ParseExact(collection["ngayBatDau"], "dd/MM/yyyy", CultureInfo.InvariantCulture);
            hopDong.ngayBatDauTinhPhep = String.IsNullOrEmpty(collection["ngayBatDauTinhPhep"]) ? (DateTime?)null : DateTime.ParseExact(collection["ngayBatDauTinhPhep"], "dd/MM/yyyy", CultureInfo.InvariantCulture);

            hopDong.ngayKetThuc = String.IsNullOrEmpty(collection["ngayKetThuc"]) ? (DateTime?)null : DateTime.ParseExact(collection["ngayKetThuc"], "dd/MM/yyyy", CultureInfo.InvariantCulture);
            hopDong.ngayKy = String.IsNullOrEmpty(collection["ngayKy"]) ? (DateTime?)null : DateTime.ParseExact(collection["ngayKy"], "dd/MM/yyyy", CultureInfo.InvariantCulture);
            hopDong.ngayLap = DateTime.Now;
            hopDong.nguoiLap = GetUser().manv;
            hopDong.doanPhi = string.IsNullOrEmpty(collection["doanPhi"]) ? 0 : Convert.ToDecimal(collection["doanPhi"].Replace('%', '0').Replace(' ', '0'));
            hopDong.dangPhi = string.IsNullOrEmpty(collection["dangPhi"]) ? 0 : Convert.ToDecimal(collection["dangPhi"].Replace('%', '0').Replace(' ', '0'));
            hopDong.tienDienThoai = string.IsNullOrEmpty(collection["tienDienThoai"]) ? 0 : Convert.ToDecimal(collection["tienDienThoai"]);
            hopDong.tienAnGiuaCa = collection["tienAnGiuaCa"].Contains("true");
            hopDong.checkPhuCapDiLai = collection["checkPhuCapDiLai"].Contains("true");
            hopDong.hopDongHetHieuLuc = collection["hopDongHetHieuLuc"].Contains("true");
            hopDong.luongDongBaoHiem = string.IsNullOrEmpty(collection["luongDongBH"]) ? 0 : Convert.ToDecimal(collection["luongDongBH"]);
            hopDong.khoanBoSungLuong = string.IsNullOrEmpty(collection["khoanBoSungLuong"]) ? 0 : Convert.ToDecimal(collection["khoanBoSungLuong"]);
            hopDong.tongLuong = string.IsNullOrEmpty(collection["tongLuong"]) ? 0 : Convert.ToDecimal(collection["tongLuong"]);
            hopDong.tenHopDong = collection["tenHopDong"];
            hopDong.tinhTrang = true;
            //  hopDong.tyLeHuong = Convert.ToDecimal(collection["tyLeHuong"].Replace('%', '0').Replace(' ', '0'));
            hopDong.luongThoaThuan = string.IsNullOrEmpty(collection["luongThoaThuan"]) ? 0 : Convert.ToDecimal(collection["luongThoaThuan"]);
            hopDong.phuCapLuong = string.IsNullOrEmpty(collection["phuCapLuong"]) ? 0 : Convert.ToDecimal(collection["phuCapLuong"]);
            hopDong.bac = Convert.ToInt32(collection["bac"]);
            hopDong.bacChucVu = Convert.ToInt32(collection["bacChucVu"]);
            hopDong.luongCoBan = string.IsNullOrEmpty(collection["luongCoBan"]) ? 0 : Convert.ToDecimal(collection["luongCoBan"]);
            hopDong.luongThanhTich = string.IsNullOrEmpty(collection["luongThanhTich"]) ? 0 : Convert.ToDecimal(collection["luongThanhTich"]);
            hopDong.hinhThucLuong = collection["maHinhThuc"];
            hopDong.baoHiem = collection["maBaoHiem"];
            hopDong.thue = collection["maThue"];
            if (hopDong.checkPhuCapDiLai == true)
            {
                hopDong.phuCapDiLaiNew = 200000;
            }
            else
            {
                hopDong.phuCapDiLaiNew = 0;
            }
            //hopDong.tienXang = string.IsNullOrEmpty(collection["tienXang"]) ? 0 : Convert.ToDecimal(collection["tienXang"]);
            //hopDong.tienDiLai = string.IsNullOrEmpty(collection["tienDiLai"]) ? 0 : Convert.ToDecimal(collection["tienDiLai"]);
        }


        //
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
                context = new LinqNhanSuDataContext();
                hopDong = context.tbl_NS_HopDongLaoDongs.Where(s => s.soHopDong == id).FirstOrDefault();
                context.tbl_NS_HopDongLaoDongs.DeleteOnSubmit(hopDong);
                context.SubmitChanges();
                LinqHeThongDataContext lqHT = new LinqHeThongDataContext();
                var nguoiDuyet = lqHT.tbl_HT_DMNguoiDuyets.Where(d => d.maPhieu == id).ToList();
                if (nguoiDuyet != null && nguoiDuyet.Count > 0)
                {
                    lqHT.tbl_HT_DMNguoiDuyets.DeleteAllOnSubmit(nguoiDuyet);
                    lqHT.SubmitChanges();
                }

                SaveActiveHistory("Xóa hợp đồng lao động: " + id);
                return RedirectToAction("Index");
            }
            catch
            {
                return View("Error");
            }
        }

        public void GetAllDropdownList()
        {
            var loaiHopDongs = context.tbl_DM_LoaiHopDongLaoDongs.ToList();
            ViewBag.LoaiHopDongs = new SelectList(loaiHopDongs, "maLoaiHopDong", "tenLoaiHopDong", model != null ? model.maLoaiHopDong : "");

            var hinhThucLuongs = context.tbl_NS_HinhThucLuongs.ToList();
            hinhThucLuongs.Insert(0, new BatDongSan.Models.NhanSu.tbl_NS_HinhThucLuong { maHinhThuc = "", tenHinhThuc = "--Chọn--" });
            ViewBag.HinhThucLuongs = new SelectList(hinhThucLuongs, "maHinhThuc", "tenHinhThuc", model != null ? model.maHinhThuc : "");

            var dmThues = context.tbl_NS_DMThues.ToList();
            dmThues.Insert(0, new BatDongSan.Models.NhanSu.tbl_NS_DMThue { maThue = "", tenThue = "--Chọn--" });
            ViewBag.DMThues = new SelectList(dmThues, "maThue", "tenThue", model != null ? model.maThue : "");

            var baoHiems = context.tbl_NS_BaoHiems.ToList();
            baoHiems.Insert(0, new BatDongSan.Models.NhanSu.tbl_NS_BaoHiem { maBaoHiem = "", tenBaoHiem = "--Chọn--" });
            ViewBag.BaoHiems = new SelectList(baoHiems, "maBaoHiem", "tenBaoHiem", model != null ? model.maBaoHiem : "");

            var thoiHanHopDongs = context.tbl_DM_ThoiHanHopDongLaoDongs.ToDictionary(x => x.id.ToString(), y => y.tenThoiHanHopDong).ToList();
            thoiHanHopDongs.Insert(0, new KeyValuePair<string, string>("", ""));
            ViewBag.ThoiHanHopDongs = new SelectList(thoiHanHopDongs, "Key", "Value", model.idThoiHanHopDong.ToString());

            var bacs = (from b in linqDM.tbl_DM_ThangLuongCongTies
                        select new
                        {
                            Key = b.bac,
                            Value = b.bac
                        }).ToList().Distinct();

            ViewBag.Bacs = new SelectList(bacs, "Key", "Value", model.bac);

            model.bac = model.bac ?? bacs.FirstOrDefault().Key;
            var bacChucVus = (from b in linqDM.tbl_DM_ThangLuongCongTies
                              where b.bac == model.bac
                              select new
                              {
                                  Key = b.capBacChucVu,
                                  Value = b.capBacChucVu
                              }).ToList();

            ViewBag.BacChucVus = new SelectList(bacChucVus, "Key", "Value", model.bacChucVu);
        }

        /// <summary>
        /// Tính phân bổ mức lương
        /// </summary>
        /// <param name="luongThoaThuan"></param>
        /// <param name="maNhanVien"></param>
        /// <returns></returns>
        public ActionResult PhanBoLuong(decimal? luongThoaThuan, string maNhanVien)
        {
            TinhPhanBoMucLuong(luongThoaThuan, maNhanVien, string.Empty);
            if (phanBoLuong.flag == 0)
            {
                return Json(new { mucLuong = "Chưa có dữ liệu trong Bảng Phân Tách Lương cho mức lương này" }, JsonRequestBehavior.AllowGet);
            }
            return PartialView("PartialPhanBoLuong", phanBoLuong);

        }

        public void TinhPhanBoMucLuong(decimal? luongThoaThuan, string maNhanVien, string soHopDong)
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

                if (!string.IsNullOrEmpty(soHopDong))
                {
                    phanBoLuong.chiTiets = (from hd in context.tbl_NS_PhuCapNhanViens
                                            join ct in context.tbl_NS_BangPhanTachMucLuongChiTiets on hd.maPhuCap equals ct.maPhuCap
                                            join b in context.tbl_NS_BangPhanTachMucLuongs on ct.maMucLuong equals b.maMucLuong
                                            join pc in context.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhuCap>() on ct.maPhuCap equals pc.maPhuCap
                                            join l in context.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_LoaiPhuCap>() on pc.loaiPhuCap equals l.id
                                            where ct.maMucLuong == mucLuong.maMucLuong && hd.soHopDong == soHopDong && hd.loaiTyLe == ct.loaiTyLe
                                            select new BangPhanTachLuongChiTietModel
                                            {
                                                ghiChu = hd.ghiChu,
                                                id = ct.id,
                                                loaiTyLe = ct.loaiTyLe,
                                                idLoaiPhuCap = pc.loaiPhuCap,
                                                tenLoaiPhuCap = l.tenLoaiPhuCap,
                                                maMucLuong = ct.maMucLuong,
                                                maPhuCap = ct.maPhuCap,
                                                salaryTemplate = hd.soTien.ToString(),
                                                tenPhuCap = pc.tenPhuCap,
                                                tenMucLuong = b.tenMucLuong,
                                                tyLe = ct.tyLe
                                            }).ToList();
                }
                else
                {
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
                }
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

        public void SavePhuCapLuong(FormCollection collection)
        {
            try
            {
                //context = new LinqNhanSuDataContext();
                phuCaps = new List<tbl_NS_PhuCapNhanVien>();
                decimal tongPhuCap = Convert.ToDecimal(collection["tongPhuCap"]);
                decimal luongCoBan = Convert.ToDecimal(collection["luongCoBan"]);
                decimal tienPhuCapKhongTheoTiLe = Convert.ToDecimal(collection["tongPhuCapKhongTheoTyLe"]);
                decimal tienPhuCapTheoTiLe = Convert.ToDecimal(collection["tongPhuCapTheoTyLe"]);
                DateTime ngayApDung = DateTime.ParseExact(collection["ngayApDung"], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                string maNhanVien = collection["maNhanVien"];
                string[] soTiens = collection.GetValues("salaryTemplate");
                string[] maPhuCaps = collection.GetValues("maPhuCap");
                string[] ghiChus = collection.GetValues("ghiChu");
                if (maPhuCaps != null)
                {
                    for (int i = 0; i < maPhuCaps.Length; i++)
                    {
                        tbl_NS_PhuCapNhanVien phuCap = new tbl_NS_PhuCapNhanVien();
                        phuCap.ghiChu = ghiChus[i];
                        phuCap.maNhanVien = maNhanVien;
                        phuCap.maPhuCap = maPhuCaps[i];
                        phuCap.ngayApDung = ngayApDung;
                        phuCap.ngayLap = DateTime.Now;
                        phuCap.nguoiLap = GetUser().manv;
                        phuCap.soTien = Convert.ToDecimal(soTiens[i]);
                        phuCap.trangThai = true;
                        phuCap.loaiTyLe = collection.GetValues("loaiTyLe")[i];
                        phuCap.soHopDong = hopDong.soHopDong;
                        phuCaps.Add(phuCap);
                    }
                    if (phuCaps.Count > 0)
                    {
                        var list = context.tbl_NS_PhuCapNhanViens.Where(s => s.soHopDong == hopDong.soHopDong);
                        context.tbl_NS_PhuCapNhanViens.DeleteAllOnSubmit(list);
                        context.tbl_NS_PhuCapNhanViens.InsertAllOnSubmit(phuCaps);
                    }

                }
            }
            catch
            {
            }
        }

        public ActionResult GetThoiHanInfo(int id)
        {
            context = new LinqNhanSuDataContext();
            var thoiHan = context.tbl_DM_ThoiHanHopDongLaoDongs.Where(s => s.id == id).FirstOrDefault();
            return Json(thoiHan);
        }

        public string IdGenerator()
        {
            return GenerateUtil.CheckLetter("HDLD", GetMax());
            //StringBuilder sb = new StringBuilder();
            //var date = DateTime.Now;
            //string lastID = context.tbl_NS_HopDongLaoDongs.OrderByDescending(d => d.id).Select(d => d.soHopDong).FirstOrDefault();
            //string nam = date.Year.ToString();
            //string ngay = date.Day.ToString();
            //nam = nam.Remove(0, 2);
            //string thang = string.Empty;
            //if (date.Month < 10)
            //{
            //    thang = "0" + date.Month;
            //}
            //else
            //{
            //    thang = date.Month.ToString();
            //}
            //if (String.IsNullOrEmpty(lastID))
            //{
            //    return "HD" + ngay + "/" + thang + "/" + nam + "/" + "001";
            //}
            //else
            //{
            //    int? idSuffix = Convert.ToInt32(lastID.Substring(lastID.Length - 3)) + 1;
            //    if (idSuffix <= 0 || idSuffix == null)
            //        return "HD" + ngay + "/" + thang + "/" + nam + "/" + "001";
            //    else
            //    {
            //        sb.Append(idSuffix.ToString());
            //        while (sb.Length < 3)
            //        {
            //            sb.Insert(0, "0");
            //        }

            //        return "HD" + ngay + "/" + thang + "/" + nam + "/" + sb.ToString();
            //    }
            //}
        }

        public string GetMax()
        {
            return context.tbl_NS_HopDongLaoDongs.OrderByDescending(d => d.ngayLap).Select(d => d.soHopDong).FirstOrDefault() ?? string.Empty;
        }

        public void SetModelData()
        {
            DMNguoiDuyetController nd = new DMNguoiDuyetController();
            model = (from hd in context.tbl_NS_HopDongLaoDongs
                     join lhd in context.tbl_DM_LoaiHopDongLaoDongs on hd.maLoaiHopDong equals lhd.maLoaiHopDong into g1
                     join th in context.tbl_DM_ThoiHanHopDongLaoDongs on hd.idThoiHanHopDong equals th.id into g2
                     join nv in context.tbl_NS_NhanViens on hd.maNhanVien equals nv.maNhanVien into g3
                     from lo in g1.DefaultIfEmpty()
                     from t in g2.DefaultIfEmpty()
                     from n in g3.DefaultIfEmpty()
                     where hd.soHopDong == hopDong.soHopDong
                     select new HopDongLaoDongModel
                     {
                         ghiChu = hd.ghiChu,
                         idThoiHanHopDong = hd.idThoiHanHopDong,
                         luongHopDong = hd.luongHopDong,
                         maChucDanh = hd.maChucDanh,
                         maLoaiHopDong = hd.maLoaiHopDong,
                         maNhanVien = hd.maNhanVien,
                         ngayBatDau = hd.ngayBatDau,
                         ngayKetThuc = hd.ngayKetThuc,
                         ngayKy = hd.ngayKy,
                         ngayLap = hd.ngayLap,
                         nguoiLap = hd.nguoiLap,
                         soHopDong = hd.soHopDong,
                         tenHopDong = hd.tenHopDong,
                         tenLoaiHopDong = lo.tenLoaiHopDong,
                         tenNhanVien = n.ho + " " + n.ten,
                         tenThoiHanHopDong = t.tenThoiHanHopDong,
                         tinhTrang = hd.tinhTrang,
                         tongLuong = hd.tongLuong,
                         tyLeHuong = hd.tyLeHuong,
                         soThang = t.soThang,
                         luongThoaThuan = hd.luongThoaThuan,
                         maQuiTrinhDuyet = hd.maQuiTrinhDuyet ?? 0,
                         Duyet = nd.GetDetailByMaPhieuTheoQuiTrinhDong(hd.soHopDong, hd.maQuiTrinhDuyet ?? 0),
                         doanPhi = (double?)hd.doanPhi ?? 0,
                         dangPhi = (double?)hd.dangPhi ?? 0,
                         tienDienThoai = hd.tienDienThoai ?? 0,
                         tienAnGiuaCa = hd.tienAnGiuaCa ?? false,
                         checkPhuCapDiLai = hd.checkPhuCapDiLai ?? false,
                         hopDongHetHieuLuc = hd.hopDongHetHieuLuc ?? false,
                         luongDongBH = (decimal?)hd.luongDongBaoHiem ?? 0,
                         khoanBoSungLuong = (decimal?)hd.khoanBoSungLuong ?? 0,
                         phuCapLuong = (double?)hd.phuCapLuong ?? 0,
                         luongCoBan = hd.luongCoBan,
                         luongThanhTich = hd.luongThanhTich,
                         bac = hd.bac,
                         bacChucVu = hd.bacChucVu,
                         tienDiLai = hd.tienDiLai,
                         tienXang = hd.tienXang,
                         maHinhThuc = hd.hinhThucLuong ?? null,
                         maBaoHiem = hd.baoHiem ?? null,
                         maThue = hd.thue ?? null,
                         phuCapDiLaiNew = (decimal?)hd.phuCapDiLaiNew ?? 0,
                         ngayBatDauTinhPhep = hd.ngayBatDauTinhPhep,

                     }).FirstOrDefault();
            var nhanVien = context.sp_NS_NhanVien_Index(null, hopDong.maNhanVien, null, null, null).FirstOrDefault();
            if (nhanVien == null)
            {
                model.tenChucDanh = string.Empty;
            }
            else
            {
                model.tenChucDanh = nhanVien.TenChucDanh;
            }
            LinqHeThongDataContext ht = new LinqHeThongDataContext();
            string hoTen = (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ho).FirstOrDefault() ?? string.Empty) + " " + (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ten).FirstOrDefault() ?? string.Empty);
            ViewBag.HoTen = hoTen;
            int trangThaiHT = (int?)ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == hopDong.soHopDong).Select(d => d.trangThai).FirstOrDefault() ?? 0;
            ViewBag.TenTrangThaiDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == trangThaiHT).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
            ViewBag.URL = Request.Url.AbsoluteUri.ToString();
            var maBuocDuyet = ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == hopDong.soHopDong).FirstOrDefault();
            ViewBag.TenBuocDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == (maBuocDuyet == null ? 0 : maBuocDuyet.trangThai)).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
        }
        public ActionResult NhanVienChuaKy()
        {
            buildTree = new StringBuilder();
            phongBans = linqDanhMuc.tbl_DM_PhongBans.ToList();
            buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
            ViewBag.NVPB = buildTree.ToString();
            return PartialView("_NhanVienPhongBan");
        }
        public ActionResult DanhSachPhongBan()
        {
            buildTree = new StringBuilder();
            phongBans = linqDanhMuc.tbl_DM_PhongBans.ToList();
            buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
            ViewBag.NVPB = buildTree.ToString();
            return PartialView("_PartDanhSachPhongBan");
        }

        public ActionResult LoadNhanVienChuaKy(int? page, string searchString, string maPhongBan)
        {
            IList<sp_PB_DanhSachNhanVienChuaKyHDResult> phongBan1s;
            phongBan1s = linqDanhMuc.sp_PB_DanhSachNhanVienChuaKyHD(searchString, maPhongBan).ToList();
            ViewBag.isGet = "True";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            ViewBag.Count = phongBan1s.Count();
            ViewBag.Search = searchString;
            ViewBag.MaPhongBan = maPhongBan;
            return PartialView("LoadNhanVienChuaKy", phongBan1s.ToPagedList(currentPageIndex, 10));
        }

        public ActionResult LoadNhanVienPhongBan(int? page, string searchString, string maPhongBan)
        {
            try
            {
                string parentID = linqDanhMuc.tbl_DM_PhongBans.Where(d => d.maPhongBan == maPhongBan).Select(d => d.maCha).FirstOrDefault() ?? string.Empty;
                if (String.IsNullOrEmpty(parentID))
                {
                    maPhongBan = string.Empty;
                }
                IList<sp_PB_DanhSachNhanVienChuaKyHDResult> phongBan1s;
                phongBan1s = linqDanhMuc.sp_PB_DanhSachNhanVienChuaKyHD(searchString, maPhongBan).ToList();
                ViewBag.isGet = "True";
                int currentPageIndex = page.HasValue ? page.Value : 1;
                ViewBag.Count = phongBan1s.Count();
                ViewBag.parrentId = maPhongBan;
                ViewBag.qSearchNV = searchString ?? string.Empty;
                return PartialView("LoadNhanVien", phongBan1s.ToPagedList(currentPageIndex, 10));
            }
            catch (Exception e)
            {
                ViewData["Message"] = e.Message;
                return View("Error");
            }
        }

        #region In Hợp đồng lao động bằng template và xuất ra file world
        public ActionResult InHopDongLaoDong(string maPhieu)
        {
            #region Role user
            permission = GetPermission("PhieuDieuChuyen", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                string noiDung = NoiDungHopDong(maPhieu);
                ViewBag.NoiDung = noiDung;
                return PartialView("InHopDongLDTemplate");
            }
            catch
            {
                return View("Error");
            }
        }

        public ActionResult In(string maPhieu)
        {
            byte[] fileContent = null;
            var utf8 = new UTF8Encoding();
            var resultFill = NoiDungHopDong(maPhieu);
            fileContent = utf8.GetBytes(resultFill);
            return File(fileContent, "application/msword", "HopDongLaoDong" + ".doc");
        }

        public string NoiDungHopDong(string maPhieu)
        {
            string noiDung = string.Empty;
            hopDong = context.tbl_NS_HopDongLaoDongs.Where(s => s.soHopDong == maPhieu).FirstOrDefault();
            SetModelData();
            var nv = context.sp_NS_NhanVien_Index(null, hopDong.maNhanVien, null, null, null).FirstOrDefault();
            var ds = lqHT.Sys_PrintTemplates.Where(d => d.maMauIn == "MIHDLD").FirstOrDefault();
            if (ds != null)
            {
                string diaChiThuongTru = nv.thuongTruTenDuong + " " + nv.thuongTruQuanHuyen + " " + nv.thuongTruTinhThanh;
                string diaChiLienLac = nv.tamTruTenDuong + " " + nv.tamTruQuanHuyen + " " + nv.tamTruTinhThanh;
                string gioiTinh = nv.gioiTinh == true ? "ông" : "bà";
                string ongHoacBa = nv.hoVaTen;
                string ngayCapCMND = nv.CMNDNgayCap.Value.Day.ToString();
                noiDung = ds.html.Replace("{$ngay}", DateTime.Now.Day.ToString())
                    .Replace("{$thang}", DateTime.Now.Month.ToString())
                    .Replace("{$nam}", DateTime.Now.Year.ToString())
                    .Replace("{$ongHoacBa}", gioiTinh)
                    .Replace("{$hoTenNhanVien}", ongHoacBa)
                    .Replace("{$ngaySinh}", nv.ngaySinh.Value.Day.ToString())
                    .Replace("{$thangSinh}", nv.ngaySinh.Value.Month.ToString())
                    .Replace("{$namSinh}", nv.ngaySinh.Value.Year.ToString())
                    .Replace("{$cmnd}", nv.CMNDSo)
                    .Replace("{$gioiTinh}", gioiTinh)
                    .Replace("{$ngayCapCMND}", ngayCapCMND)
                    .Replace("{$thangCapCMND}", nv.CMNDNgayCap.Value.Month.ToString())
                    .Replace("{$namCapCMND}", nv.CMNDNgayCap.Value.Year.ToString())
                    .Replace("{$congAnCapCMND}", nv.CMNDNoiCap)
                    .Replace("{$dangKyHKTT}", diaChiThuongTru)
                    .Replace("{$choOHienNay}", diaChiLienLac)
                    .Replace("{$chucVu}", nv.TenChucDanh)
                    .Replace("{$thoiHanHopDong}", model.soThang.ToString())
                    .Replace("{$ngaykyHopDong}", model.ngayKy.Value.Day.ToString())
                    .Replace("{$thangKyHopDong}", model.ngayKy.Value.Year.ToString())
                    .Replace("{$namKyHopDong}", model.ngayKy.Value.Year.ToString())
                    .Replace("{$ngayKT}", model.ngayKetThuc.Value.Day.ToString())
                    .Replace("{$thangKT}", model.ngayKetThuc.Value.Year.ToString())
                    .Replace("{$namKT}", model.ngayKetThuc.Value.Year.ToString())

                    .Replace("{$mucLuongChinh}", model.tongLuong.Value.ToString("#,##0"))
                    .Replace("{$tienDienThoai}", model.tienDienThoai.ToString("#,##0"))
                    .Replace("{$tienAnGiuaCa}", model.tienAnGiuaCa.ToString())
                    .Replace("{$tienPhuCap}", model.phuCapLuong.ToString("#,##0"))
                    ;
            }
            return noiDung;
        }
        #endregion

        #region Import file excel
        public FileResult DownloadImportFile()
        {
            string savedFileName = Path.Combine("/UploadFiles/Template/", "PhieuNhapHopDongLaoDong.xls");
            return File(savedFileName, "multipart/form-data", "PhieuNhapHopDongLaoDong.xls");
        }

        [HttpPost]
        public ActionResult ImportExcelData(string excelPath)
        {
            string fileName = string.Empty;
            try
            {
                List<tbl_NS_HopDongLaoDong> listHD = new List<tbl_NS_HopDongLaoDong>();
                string flag = "true";
                string[] supportedFiles = { ".xlsx", ".xls" };
                HttpPostedFileBase File;
                File = Request.Files[0];
                if (File.ContentLength > 0)
                {
                    string extension = Path.GetExtension(File.FileName);
                    bool exist = Array.Exists(supportedFiles, element => element == extension);
                    if (exist == false)
                    {
                        return Json(new { success = false });
                    }
                    else
                    {
                        var date = DateTime.Now.ToString("yyyyMMdd-HHMMss");
                        string savedLocation = "/UploadFiles/NhanVien/";
                        Directory.CreateDirectory(savedLocation);
                        var filePath = Server.MapPath(savedLocation);
                        fileName = date.ToString() + File.FileName;
                        string savedFileName = Path.Combine(filePath, fileName);
                        File.SaveAs(savedFileName);

                        ExcelDataProcessing excelDataProcessor = new ExcelDataProcessing(savedFileName);
                        DataTable dt = excelDataProcessor.GetDataTableWorkSheet("NhanVien");
                        if (dt != null && dt.Rows.Count >= 1)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                tbl_NS_HopDongLaoDong hd = new tbl_NS_HopDongLaoDong();
                                if (String.IsNullOrEmpty(row["Mã nhân viên"].ToString()))
                                {
                                    flag = "false";
                                    return Json(new { success = "Xin bạn vui lòng nhập mã nhân viên trước khi lập hợp đồng" });
                                }
                                else
                                {
                                    //Kiểm tra mã nhân viên có tồn tại hay chưa
                                    var nv = context.tbl_NS_NhanViens.Where(d => d.maNhanVien == row["Mã nhân viên"].ToString()).FirstOrDefault();
                                    if (nv == null)
                                    {
                                        flag = "false";
                                        return Json(new { success = "Mã nhân viên " + row["Mã nhân viên"].ToString() + " không tồn tại! Xin bạn vui lòng nhập nhân viên trước khi lập hợp đồng" });
                                    }
                                }
                                hd.soHopDong = IdGenerator();
                                hd.tenHopDong = row["Tên hợp đồng"].ToString();
                                hd.ngayKy = String.IsNullOrEmpty(row["Ngày ký hợp đồng"].ToString()) ? (DateTime?)null : DateTime.ParseExact(row["Ngày ký hợp đồng"].ToString(), "d/M/yyyy", CultureInfo.InvariantCulture);
                                hd.maNhanVien = row["Mã nhân viên"].ToString();
                                try
                                {
                                    hd.idThoiHanHopDong = Convert.ToByte(row["Thời hạn hợp đồng"]);
                                }
                                catch
                                {
                                    flag = "false";
                                    return Json(new { success = "Thời hạn hợp đồng không đúng định dạnh xin vui lòng nhập lại" });
                                }
                                hd.luongThoaThuan = (String.IsNullOrEmpty(row["Lương"].ToString()) ? 0 : Convert.ToDecimal(row["Lương"].ToString().Replace(",", " ")));
                                hd.phuCapLuong = (String.IsNullOrEmpty(row["Phụ cấp lương"].ToString()) ? 0 : Convert.ToDecimal(row["Phụ cấp lương"].ToString().Replace(",", " ")));
                                hd.khoanBoSungLuong = (String.IsNullOrEmpty(row["Khoản bổ sung lương"].ToString()) ? 0 : Convert.ToDecimal(row["Khoản bổ sung lương"].ToString().Replace(",", " ")));
                                hd.luongDongBaoHiem = hd.luongThoaThuan + hd.phuCapLuong;
                                hd.tongLuong = hd.luongDongBaoHiem + hd.khoanBoSungLuong;
                                hd.ngayBatDau = String.IsNullOrEmpty(row["Ngày bắt đầu"].ToString()) ? (DateTime?)null : DateTime.ParseExact(row["Ngày bắt đầu"].ToString(), "d/M/yyyy", CultureInfo.InvariantCulture);
                                hd.ngayKetThuc = String.IsNullOrEmpty(row["Ngày kết thúc"].ToString()) ? (DateTime?)null : DateTime.ParseExact(row["Ngày kết thúc"].ToString(), "d/M/yyyy", CultureInfo.InvariantCulture);
                                hd.ghiChu = row["Ghi chú"].ToString();
                                hd.nguoiLap = GetUser().manv;
                                hd.maLoaiHopDong = row["Loại hợp đồng"].ToString();
                                if (!string.IsNullOrEmpty(hd.maLoaiHopDong))
                                {
                                    var dsLoaiHD = context.tbl_DM_LoaiHopDongLaoDongs.Where(d => d.maLoaiHopDong.Trim() == hd.maLoaiHopDong.Trim()).FirstOrDefault();
                                    if (dsLoaiHD == null)
                                    {
                                        flag = "false";
                                        return Json(new { success = "Loại hợp đồng " + hd.maLoaiHopDong + " không đúng! Xin bạn vui lòng nhập loại hợp đồng cho đúng" });
                                    }
                                }
                                else
                                {
                                    flag = "false";
                                    return Json(new { success = "Xin bạn vui lòng nhập loại hợp đồng" });
                                }
                                hd.doanPhi = (String.IsNullOrEmpty(row["Đoàn phí"].ToString()) ? 0 : Convert.ToDecimal(row["Đoàn phí"].ToString()));
                                hd.dangPhi = (String.IsNullOrEmpty(row["Đảng phí"].ToString()) ? 0 : Convert.ToDecimal(row["Đảng phí"].ToString()));
                                string tienAnGC = row["Tiền ăn giữa ca"].ToString();
                                if (tienAnGC == "C" && !string.IsNullOrEmpty(tienAnGC))
                                {
                                    hd.tienAnGiuaCa = true;
                                }
                                else if (tienAnGC == "K" && !string.IsNullOrEmpty(tienAnGC))
                                {
                                    hd.tienAnGiuaCa = false;
                                }
                                else
                                {
                                    flag = "false";
                                    return Json(new { success = "Xin bạn vui lòng nhập lại tiền ăn giữa ca cho đúng" });
                                }
                                hd.tienDienThoai = (String.IsNullOrEmpty(row["Tiền điện thoại"].ToString()) ? 0 : Convert.ToDecimal(row["Tiền điện thoại"].ToString().Replace(",", " ")));
                                listHD.Add(hd);
                            }
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(fileName))
                            {
                                System.IO.File.Delete(Server.MapPath("/UploadFiles/NhanVien/" + fileName));
                                return Json(new { success = "Không có nhân viên để lập hợp đồng" });
                            }
                        }
                        if (flag == "true" && listHD != null && listHD.Count > 0)
                        {
                            foreach (var item in listHD)
                            {
                                tbl_NS_HopDongLaoDong hd = new tbl_NS_HopDongLaoDong();
                                var dshopDong = context.tbl_NS_HopDongLaoDongs.Where(d => d.maNhanVien == item.maNhanVien).FirstOrDefault();
                                if (dshopDong == null)
                                {
                                    hd.soHopDong = IdGenerator();
                                    hd.tenHopDong = item.tenHopDong;
                                    hd.ngayKy = item.ngayKy;
                                    hd.maNhanVien = item.maNhanVien;
                                    hd.idThoiHanHopDong = item.idThoiHanHopDong;
                                    hd.luongThoaThuan = item.luongThoaThuan;
                                    hd.phuCapLuong = item.phuCapLuong;
                                    hd.luongDongBaoHiem = item.luongDongBaoHiem;
                                    hd.khoanBoSungLuong = item.khoanBoSungLuong;
                                    hd.tongLuong = item.tongLuong;
                                    hd.ngayBatDau = item.ngayBatDau;
                                    hd.ngayKetThuc = item.ngayKetThuc;
                                    hd.ghiChu = item.ghiChu;
                                    hd.nguoiLap = GetUser().manv;
                                    hd.maLoaiHopDong = item.maLoaiHopDong;
                                    hd.doanPhi = item.doanPhi;
                                    hd.dangPhi = item.dangPhi;
                                    hd.ngayLap = DateTime.Now;
                                    hd.tienAnGiuaCa = item.tienAnGiuaCa;
                                    hd.tienDienThoai = item.tienDienThoai;
                                    context.tbl_NS_HopDongLaoDongs.InsertOnSubmit(hd);
                                    context.SubmitChanges();
                                }
                            }
                            System.IO.File.Delete(Server.MapPath("/UploadFiles/NhanVien/" + fileName));
                        }

                    }
                }
                return Json(new { success = true });
            }
            catch
            {
                if (!String.IsNullOrEmpty(fileName))
                {
                    System.IO.File.Delete(Server.MapPath("/UploadFiles/NhanVien/" + fileName));
                }
                return Json(new { success = "Đã xảy ra lỗi trong quá trình import" });
            }
        }

        #endregion

        public ActionResult GetBacChucVu(int id)
        {
            try
            {
                var bacChucVus = linqDM.tbl_DM_ThangLuongCongTies.Where(s => s.bac == id).ToList();
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
                var thangLuongs = linqDM.tbl_DM_ThangLuongCongTies.Where(s => s.bac == bac && s.capBacChucVu == bacChucVu).ToList();
                model = new HopDongLaoDongModel();
                model.thangLuongs = thangLuongs;
                return PartialView("PartialThangLuong", model);
            }
            catch
            {
                return View();
            }
        }

        #region Xuất excel
        public void XuatFileHopDong()
        {
            var filename = "";
            var virtualPath = HttpRuntime.AppDomainAppVirtualPath;
            var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplate.xls"), FileMode.Open, FileAccess.Read);

            var workbook = new HSSFWorkbook(fileStream, true);
            filename += "DanhSachHopDong.xls";


            var sheet = workbook.GetSheet("danhsachnhanvien");

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

            string rowtitle = "DANH SÁCH HỢP ĐỒNG";
            var titleCell = HSSFCellUtil.CreateCell(sheet.CreateRow(firstRowNumber), 6, rowtitle.ToUpper());
            titleCell.CellStyle = styleTitle;

            ++firstRowNumber;

            var list1 = new List<string>();
            list1.Add("STT");
            list1.Add("Mã hợp đồng");
            list1.Add("Số hợp đồng");
            list1.Add("Mã nhân viên");
            list1.Add("Tên nhân viên");
            list1.Add("Phòng ban");
            list1.Add("Chức danh");
            list1.Add("Ngày ký hợp đồng");
            list1.Add("Thời hạn hợp đồng");
            list1.Add("Tổng lương");
            list1.Add("Lương đóng BH");
            list1.Add("Qũy nộ bộ");
            list1.Add("Phụ cấp thu hút");
            list1.Add("Tiền ăn giữa ca");

            //Start row 13
            var headerRow = sheet.CreateRow(2);
            ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);

            //Create header end
            context = new LinqNhanSuDataContext();
            var idRowStart = 3;
            var datas = context.sp_NS_HopDongLaoDong_Index(null, null, null, null, null, null, null, null).ToList();
            //#region
            if (datas != null && datas.Count > 0)
            {
                var stt = 0;
                int dem = 0;
                //Giai đoạn
                foreach (var item in datas)
                {
                    dem = 0;

                    idRowStart++;
                    rowC = sheet.CreateRow(idRowStart);
                    ReportHelperExcel.SetAlignment(rowC, dem++, (++stt).ToString(), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item.soHopDong, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item.tenHopDong, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item.maNhanVien, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, (item.hoTenNhanVien), hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item.tenPhongBan, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item.TenChucDanh, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:dd/MM/yyyy}", item.ngayKy), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item.tenThoiHanHopDong, hStyleConRight);

                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.###}", item.tongLuong), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.###}", item.luongDongBaoHiem), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.###}", item.doanPhi), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.###}", item.khoanBoSungLuong), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, (item.tienAnGiuaCa == true ? "Có" : "Không"), hStyleConCenter);

                }

                sheet.SetColumnWidth(0, 5 * 250);
                sheet.SetColumnWidth(1, 20 * 270);
                sheet.SetColumnWidth(2, 20 * 250);
                sheet.SetColumnWidth(3, 10 * 210);
                sheet.SetColumnWidth(4, 25 * 210);
                sheet.SetColumnWidth(5, 30 * 210);
                sheet.SetColumnWidth(6, 30 * 210);
                sheet.SetColumnWidth(7, 30 * 210);
                sheet.SetColumnWidth(8, 30 * 210);
                sheet.SetColumnWidth(9, 20 * 210);
                sheet.SetColumnWidth(10, 20 * 210);
                sheet.SetColumnWidth(11, 20 * 210);
                sheet.SetColumnWidth(12, 20 * 210);
                sheet.SetColumnWidth(13, 15 * 210);
            }
            else
            {

                sheet.SetColumnWidth(0, 5 * 250);
                sheet.SetColumnWidth(1, 15 * 270);
                sheet.SetColumnWidth(2, 20 * 250);
                sheet.SetColumnWidth(3, 10 * 210);
                sheet.SetColumnWidth(4, 15 * 210);
                sheet.SetColumnWidth(5, 30 * 210);
                sheet.SetColumnWidth(6, 30 * 210);
                sheet.SetColumnWidth(7, 30 * 210);
                sheet.SetColumnWidth(8, 30 * 210);
                sheet.SetColumnWidth(9, 30 * 210);
                sheet.SetColumnWidth(10, 20 * 210);
                sheet.SetColumnWidth(11, 15 * 210);
                sheet.SetColumnWidth(12, 15 * 210);
                sheet.SetColumnWidth(13, 15 * 210);
            }

            var stream = new MemoryStream();
            workbook.Write(stream);

            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", filename));
            Response.Clear();

            Response.BinaryWrite(stream.GetBuffer());
            Response.End();

        }
        #endregion

        #region Tạo phụ lục tự động
        public void TaoPhuLucTuDong()
        {
            var listLDBH = linqDM.BangThayDoiBaoHiemXaHoi2026s.OrderBy(d => d.STT).ToList();
            int count = 0;
            int kq = 0;
            DateTime ngayLap = DateTime.Now;
            List<tbl_NS_PhuLucHopDong> listPL = new List<tbl_NS_PhuLucHopDong>();
            List<tbl_HT_DMNguoiDuyet> listND = new List<tbl_HT_DMNguoiDuyet>();
            tbl_HT_DMNguoiDuyet nd;
            tbl_NS_PhuLucHopDong ct;
            foreach (var item in listLDBH)
            {
                string maHopDongLD = linqDM.GetTable<tbl_NS_HopDongLaoDong>().Where(d => d.maNhanVien.Trim() == item.MaNhanVien.Trim()).Select(d => d.soHopDong).FirstOrDefault();
                if (!string.IsNullOrEmpty(maHopDongLD))
                {
                    var dsHopDong = context.tbl_NS_HopDongLaoDongs.Where(d => d.soHopDong == maHopDongLD).FirstOrDefault();
                    if (dsHopDong != null)
                    {
                        ct = new tbl_NS_PhuLucHopDong();
                        //Kiểm tra phụ lục đã có chưa
                        var phuLuc = context.tbl_NS_PhuLucHopDongs.Where(d => d.soHopDong == dsHopDong.soHopDong).OrderByDescending(d => d.ngayLap).FirstOrDefault();
                        if (phuLuc != null)
                        {
                            ct.soPhuLuc = IdGeneratorPL(dsHopDong.soHopDong);
                            ct.noiDungThayDoi = "Cập nhật BHXH từ 1/1/2026";
                            ct.ngayHieuLuc = Convert.ToDateTime("2026-01-01");
                            //phuLuc.ngayHieuLuc;
                            ct.ghiChu = phuLuc.ghiChu;
                            ct.nguoiLap = phuLuc.nguoiLap;
                            ct.ngayLap = ngayLap;
                            ct.giaHanDen = Convert.ToDateTime("2028-12-30");
                                //DateTime.ParseExact("2028-12-30", "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                            //phuLuc.giaHanDen;
                            ct.soHopDong = phuLuc.soHopDong;
                            ct.maQuiTrinhDuyet = phuLuc.maQuiTrinhDuyet;
                            ct.doanPhi = phuLuc.doanPhi;
                            ct.dangPhi = phuLuc.dangPhi;
                            ct.tienDienThoai = phuLuc.tienDienThoai;
                            ct.tienAnGiuaCa = phuLuc.tienAnGiuaCa;
                            ct.luongDongBaoHiem = (decimal?)item.LuongDongBaoHiem ?? 0;
                            ct.khoanBoSungLuong = phuLuc.khoanBoSungLuong;
                            ct.luong = phuLuc.luong;
                            ct.phuCapLuong = phuLuc.phuCapLuong;
                            ct.tongLuong = phuLuc.tongLuong;
                            ct.luongCoBan = phuLuc.luongCoBan;
                            ct.luongThanhTich = phuLuc.luongThanhTich;
                            ct.bac = phuLuc.bac;
                            ct.bacChucVu = phuLuc.bacChucVu;
                            ct.tienXang = phuLuc.tienXang;
                            ct.tienDiLai = phuLuc.tienDiLai;
                            ct.hinhThucLuong = phuLuc.hinhThucLuong;
                            ct.baoHiem = phuLuc.baoHiem;
                            ct.thue = phuLuc.thue;
                            ct.maLoaiHopDong = phuLuc.maLoaiHopDong;
                            ct.phuCapDiLaiNew = phuLuc.phuCapDiLaiNew;
                            ct.checkPhuCapDiLai = phuLuc.checkPhuCapDiLai;
                            ct.ngayBatDauTinhPhep = phuLuc.ngayBatDauTinhPhep;
                        }
                        else
                        {
                            ct.soPhuLuc = IdGeneratorPL(dsHopDong.soHopDong);
                            ct.noiDungThayDoi = "Cập nhật BHXH từ 1/1/2026";
                            ct.ngayHieuLuc = Convert.ToDateTime("2026-01-01");

                            //'2024-07-01 00:00:00.000';
                            ct.ghiChu = hopDong.ghiChu;
                            ct.nguoiLap = hopDong.nguoiLap;
                            ct.ngayLap = ngayLap;
                            ct.giaHanDen = Convert.ToDateTime("2028-12-30");
                            ct.soHopDong = hopDong.soHopDong;
                            ct.maQuiTrinhDuyet = hopDong.maQuiTrinhDuyet;
                            ct.doanPhi = hopDong.doanPhi;
                            ct.dangPhi = hopDong.dangPhi;
                            ct.tienDienThoai = hopDong.tienDienThoai;
                            ct.tienAnGiuaCa = hopDong.tienAnGiuaCa;
                            ct.luongDongBaoHiem = (decimal?)item.LuongDongBaoHiem ?? 0;
                            ct.khoanBoSungLuong = hopDong.khoanBoSungLuong;
                            ct.luong = 0;
                            ct.phuCapLuong = hopDong.phuCapLuong;
                            ct.tongLuong = hopDong.tongLuong;
                            ct.luongCoBan = hopDong.luongCoBan;
                            ct.luongThanhTich = hopDong.luongThanhTich;
                            ct.bac = hopDong.bac;
                            ct.bacChucVu = hopDong.bacChucVu;
                            ct.tienXang = hopDong.tienXang;
                            ct.tienDiLai = hopDong.tienDiLai;
                            ct.hinhThucLuong = hopDong.hinhThucLuong;
                            ct.baoHiem = hopDong.baoHiem;
                            ct.thue = hopDong.thue;
                            ct.maLoaiHopDong = hopDong.maLoaiHopDong;
                            ct.phuCapDiLaiNew = hopDong.phuCapDiLaiNew;
                            ct.checkPhuCapDiLai = hopDong.checkPhuCapDiLai;
                            ct.ngayBatDauTinhPhep = hopDong.ngayBatDauTinhPhep;
                        }
                        listPL.Add(ct);
                        nd = new tbl_HT_DMNguoiDuyet();
                        nd.maCongViec = "PhuLucHopDongLaoDong";
                        nd.maPhieu = ct.soPhuLuc;
                        nd.maNV = "NV-320";
                        nd.trangThai = 2;
                        nd.ngayDuyet = ngayLap;
                        nd.lyDo = "Thay đổi mức đóng bảo hiểm xã hội";
                        listND.Add(nd);

                    }
                    count++;
                }

            }
            linqDM.GetTable<tbl_NS_PhuLucHopDong>().InsertAllOnSubmit(listPL);
            linqDM.GetTable<tbl_HT_DMNguoiDuyet>().InsertAllOnSubmit(listND);
            linqDM.SubmitChanges();
            kq = count;
        }

        public string IdGeneratorPL(string soHopDong)
        {
            StringBuilder sb = new StringBuilder();
            var date = DateTime.Now;
            int soLanTaoPhuLuc = context.tbl_NS_PhuLucHopDongs.Where(s => s.soHopDong == soHopDong).Count() + 1;
            return soHopDong + "-PLHD-" + soLanTaoPhuLuc.ToString();
        }
        #endregion
    }
}
