using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Helper.Utils;
using BatDongSan.Models.NhanSu;
using BatDongSan.Models.PhieuDeNghi;
using BatDongSan.Models.HeThong;
using BatDongSan.Models.DanhMuc;
using System.Text;
using BatDongSan.Utils.Paging;
using BatDongSan.Helper.Common;
using System.Globalization;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel.Contrib;
using Worldsoft.Mvc.Web.Util;
using System.Data;

namespace BatDongSan.Controllers.NhanSu
{
    public class PhieuTamUngController : ApplicationController
    {
        LinqPhieuDeNghiDataContext lqPhieuDN = new LinqPhieuDeNghiDataContext();
        LinqNhanSuDataContext linqNS = new LinqNhanSuDataContext();
        LinqDanhMucDataContext linqDM = new LinqDanhMucDataContext();
        tbl_NS_PhieuTamUng tblPhieuDeNghi;
        IList<tbl_NS_PhieuTamUng> tblPhieuDeNghis;
        PhieuTamUng PhieuDeNghiModel;
        public const string taskIDSystem = "PhieuTamUng";//REGWORKVOTE
        public bool? permission;
        //
        // GET: /PhieuCongTac/

        public ActionResult Index(int? page, string searchString, int? thang, int? nam, string trangThai)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            BindDataTrangThai(taskIDSystem);
            var userName = GetUser().manv;
            FunThang(0);
            FunNam(DateTime.Now.Year);

            ViewBag.isGet = "True";
            var tblPhieuDeNghis = linqNS.sp_NS_PhieuTamUng_Index(thang, nam, searchString, trangThai).ToList();
            int currentPageIndex = page.HasValue ? page.Value : 1;
            ViewBag.Count = tblPhieuDeNghis.Count();
            ViewBag.Search = searchString;
            ViewBag.thang = thang;
            ViewBag.nam = nam;
            ViewBag.trangThai = trangThai;
            ViewBag.maNhanVien = GetUser().manv;
            return View(tblPhieuDeNghis.ToPagedList(currentPageIndex, 50));

        }
        public ActionResult ViewIndex(int? page, string qSearch, int? thang, int? nam, string trangThai)
        {
            var userName = GetUser().manv;


            ViewBag.isGet = "True";
            var tblPhieuDeNghis = linqNS.sp_NS_PhieuTamUng_Index(thang, nam, qSearch, trangThai).ToList();
            int currentPageIndex = page.HasValue ? page.Value : 1;
            ViewBag.Count = tblPhieuDeNghis.Count();
            ViewBag.Search = qSearch;
            ViewBag.thang = thang;
            ViewBag.nam = nam;
            ViewBag.trangThai = trangThai;
            ViewBag.maNhanVien = GetUser().manv;
            return PartialView("ViewIndex", tblPhieuDeNghis.ToPagedList(currentPageIndex, 50));

        }

        public ActionResult Create()
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenThem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            PhieuDeNghiModel = new PhieuTamUng();
            PhieuDeNghiModel.maPhieu = GenerateUtil.CheckLetter("DNTU", GetMax());
            PhieuDeNghiModel.ngayLap = DateTime.Now;
            PhieuDeNghiModel.NhanVienLapPhieu = new BatDongSan.Models.NhanSu.NhanVienModel
            {
                maNhanVien = GetUser().manv,
                hoVaTen = HoVaTen(GetUser().manv)
            };
            PhieuDeNghiModel.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel();
            PhieuDeNghiModel.soTien = 0;

            builtThang(DateTime.Now.Month);
            builtNam(DateTime.Now.Year);

            return View(PhieuDeNghiModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(FormCollection coll)
        {
            try
            {
                tblPhieuDeNghi = new tbl_NS_PhieuTamUng();
                tblPhieuDeNghi.soPhieu = GenerateUtil.CheckLetter("DNTU", GetMax());
                tblPhieuDeNghi.nguoiLap = GetUser().manv;
                tblPhieuDeNghi.ngayTamUng = String.IsNullOrEmpty(coll.Get("ngayTamUng")) ? (DateTime?)null : DateTime.ParseExact(coll.Get("ngayTamUng"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                tblPhieuDeNghi.ngayLap = DateTime.Now;
                tblPhieuDeNghi.maNhanVien = coll.Get("NhanVien.maNhanVien");

                tblPhieuDeNghi.tamUngThang = Convert.ToInt32(coll.Get("thang"));
                tblPhieuDeNghi.tamUngNam = Convert.ToInt32(coll.Get("nam"));
                tblPhieuDeNghi.soTien = Convert.ToDouble(coll.Get("soTien"));
                tblPhieuDeNghi.ghiChu = coll.Get("ghiChu");

                linqNS.tbl_NS_PhieuTamUngs.InsertOnSubmit(tblPhieuDeNghi);
                linqNS.SubmitChanges();

                return RedirectToAction("Edit", new { id = tblPhieuDeNghi.soPhieu });
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }
        public string GetMax()
        {
            return linqNS.tbl_NS_PhieuTamUngs.OrderByDescending(d => d.soPhieu).Select(d => d.soPhieu).FirstOrDefault() ?? string.Empty;
        }
        [HttpPost]
        public ActionResult Delete(string id)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenXoa);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                var phieu = linqNS.tbl_NS_PhieuTamUngs.Where(d => d.soPhieu == id).FirstOrDefault();
                linqNS.tbl_NS_PhieuTamUngs.DeleteOnSubmit(phieu);
                linqNS.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View("error");
            }
        }
        public ActionResult Edit(string id)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            var dataPhieuCongTac = linqNS.tbl_NS_PhieuTamUngs.Where(d => d.soPhieu == id).FirstOrDefault();
            PhieuDeNghiModel = new PhieuTamUng();
            PhieuDeNghiModel.maPhieu = dataPhieuCongTac.soPhieu;
            PhieuDeNghiModel.ngayLap = dataPhieuCongTac.ngayLap;
            PhieuDeNghiModel.nguoiLap = dataPhieuCongTac.nguoiLap;
            PhieuDeNghiModel.ngayTamUng = dataPhieuCongTac.ngayTamUng;
            PhieuDeNghiModel.maQuiTrinhDuyet = dataPhieuCongTac.maQuiTrinhDuyet ?? 0;
            PhieuDeNghiModel.NhanVienLapPhieu = new BatDongSan.Models.NhanSu.NhanVienModel
            {
                maNhanVien = dataPhieuCongTac.nguoiLap,
                hoVaTen = HoVaTen(dataPhieuCongTac.nguoiLap)
            };

            PhieuDeNghiModel.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel
            {
                maNhanVien = dataPhieuCongTac.maNhanVien,
                hoVaTen = HoVaTen(dataPhieuCongTac.maNhanVien)
            };
            PhieuDeNghiModel.soTien = (decimal?)dataPhieuCongTac.soTien;
            PhieuDeNghiModel.tamUngThang = dataPhieuCongTac.tamUngThang;
            PhieuDeNghiModel.tamUngNam = dataPhieuCongTac.tamUngNam;
            PhieuDeNghiModel.ghiChu = dataPhieuCongTac.ghiChu;

            builtNam(dataPhieuCongTac.tamUngNam ?? 0);
            builtThang(dataPhieuCongTac.tamUngThang ?? 0);
            DMNguoiDuyetController nd = new DMNguoiDuyetController();
            PhieuDeNghiModel.Duyet = nd.GetDetailByMaPhieuTheoQuiTrinhDong(PhieuDeNghiModel.maPhieu, PhieuDeNghiModel.maQuiTrinhDuyet);
            LinqHeThongDataContext ht = new LinqHeThongDataContext();
            string hoTen = (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ho).FirstOrDefault() ?? string.Empty) + " " + (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ten).FirstOrDefault() ?? string.Empty);
            ViewBag.HoTen = hoTen;
            int trangThaiHT = (int?)ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id).Select(d => d.trangThai).FirstOrDefault() ?? 0;
            ViewBag.TenTrangThaiDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == trangThaiHT).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
            ViewBag.URL = Request.Url.AbsoluteUri.ToString();
            var maBuocDuyet = ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id).FirstOrDefault();
            ViewBag.TenBuocDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == (maBuocDuyet == null ? 0 : maBuocDuyet.trangThai)).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
            return View(PhieuDeNghiModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(string id, FormCollection coll)
        {
            try
            {
                tblPhieuDeNghi = linqNS.tbl_NS_PhieuTamUngs.Where(d => d.soPhieu == id).FirstOrDefault();
                tblPhieuDeNghi.ngayTamUng = String.IsNullOrEmpty(coll.Get("ngayTamUng")) ? (DateTime?)null : DateTime.ParseExact(coll.Get("ngayTamUng"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                tblPhieuDeNghi.tamUngThang = Convert.ToInt32(coll.Get("thang"));
                tblPhieuDeNghi.tamUngNam = Convert.ToInt32(coll.Get("nam"));
                tblPhieuDeNghi.soTien = Convert.ToDouble(coll.Get("soTien"));
                tblPhieuDeNghi.ghiChu = coll.Get("ghiChu");
                linqNS.SubmitChanges();

                return RedirectToAction("Edit", new { id = tblPhieuDeNghi.soPhieu });
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }


        public ActionResult Details(string id)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            var dataPhieuCongTac = linqNS.tbl_NS_PhieuTamUngs.Where(d => d.soPhieu == id).FirstOrDefault();
            PhieuDeNghiModel = new PhieuTamUng();
            PhieuDeNghiModel.ngayLap = dataPhieuCongTac.ngayLap;
            PhieuDeNghiModel.nguoiLap = dataPhieuCongTac.nguoiLap;
            PhieuDeNghiModel.maPhieu = dataPhieuCongTac.soPhieu;
            PhieuDeNghiModel.ngayTamUng = dataPhieuCongTac.ngayTamUng;
            PhieuDeNghiModel.maQuiTrinhDuyet = dataPhieuCongTac.maQuiTrinhDuyet ?? 0;
            PhieuDeNghiModel.NhanVienLapPhieu = new BatDongSan.Models.NhanSu.NhanVienModel
            {
                maNhanVien = dataPhieuCongTac.nguoiLap,
                hoVaTen = HoVaTen(dataPhieuCongTac.nguoiLap)
            };

            PhieuDeNghiModel.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel
            {
                maNhanVien = dataPhieuCongTac.maNhanVien,
                hoVaTen = HoVaTen(dataPhieuCongTac.maNhanVien)
            };
            PhieuDeNghiModel.tamUngThang = dataPhieuCongTac.tamUngThang;
            PhieuDeNghiModel.tamUngNam = dataPhieuCongTac.tamUngNam;
            PhieuDeNghiModel.ngayLap = dataPhieuCongTac.ngayLap;
            PhieuDeNghiModel.ghiChu = dataPhieuCongTac.ghiChu;
            PhieuDeNghiModel.soTien = Convert.ToDecimal(dataPhieuCongTac.soTien);
            DMNguoiDuyetController nd = new DMNguoiDuyetController();
            PhieuDeNghiModel.Duyet = nd.GetDetailByMaPhieuTheoQuiTrinhDong(PhieuDeNghiModel.maPhieu, PhieuDeNghiModel.maQuiTrinhDuyet);
            LinqHeThongDataContext ht = new LinqHeThongDataContext();
            string hoTen = (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ho).FirstOrDefault() ?? string.Empty) + " " + (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ten).FirstOrDefault() ?? string.Empty);
            ViewBag.HoTen = hoTen;
            int trangThaiHT = (int?)ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id).Select(d => d.trangThai).FirstOrDefault() ?? 0;
            ViewBag.TenTrangThaiDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == trangThaiHT).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
            ViewBag.URL = Request.Url.AbsoluteUri.ToString();
            var maBuocDuyet = ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id).FirstOrDefault();
            ViewBag.TenBuocDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == (maBuocDuyet == null ? 0 : maBuocDuyet.trangThai)).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
            return View(PhieuDeNghiModel);
        }
        private void FunThang(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 0; i < 13; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["lstThang"] = new SelectList(dics, "Key", "Value", value);
        }
        private void FunNam(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 2015; i < 2031; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["lstNam"] = new SelectList(dics, "Key", "Value", value);
        }
        public FileResult DownloadImportFile()
        {
            string savedFileName = Path.Combine("/UploadFiles/Template/", "PhieuTamUng.xlsx");
            return File(savedFileName, "multipart/form-data", "PhieuTamUng.xlsx");
        }
        public ActionResult ImportExcelData(string excelPath)
        {
            try
            {
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
                        string savedLocation = "/UploadFiles/PhieuTamUng/";
                        Directory.CreateDirectory(savedLocation);
                        var filePath = Server.MapPath(savedLocation);
                        string fileName = date.ToString() + File.FileName;
                        string savedFileName = Path.Combine(filePath, fileName);
                        File.SaveAs(savedFileName);

                        ExcelDataProcessing excelDataProcessor = new ExcelDataProcessing(savedFileName);
                        DataTable dt = excelDataProcessor.GetDataTableWorkSheet("PhieuTamUng");

                        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                        {
                            List<PhieuTamUng> lstPhieuTamUng = new List<PhieuTamUng>();
                            PhieuTamUng ptu_ = null;

                            foreach (DataRow row in dt.Rows)
                            {
                                if (string.IsNullOrEmpty(row["Tên nhân viên"].ToString()))
                                    continue;

                                ptu_ = new PhieuTamUng();

                                ptu_.sTT = String.IsNullOrEmpty(row["STT"].ToString()) ? 0 : Convert.ToInt32(row["STT"]);
                                ptu_.soTaiKhoan = row["Số tài khoản"].ToString();
                                ptu_.ngayTamUng = String.IsNullOrEmpty(row["Ngày thu tạm ứng"].ToString()) ? (DateTime?)null : DateTime.Parse(row["Ngày thu tạm ứng"].ToString());
                                ptu_.tamUngNam = String.IsNullOrEmpty(row["Năm"].ToString()) ? 0 : Convert.ToInt32(row["Năm"]);
                                ptu_.tamUngThang = String.IsNullOrEmpty(row["Tháng"].ToString()) ? 0 : Convert.ToInt32(row["Tháng"]);
                                ptu_.soTien = String.IsNullOrEmpty(row["Số tiền TU"].ToString()) ? 0 : Convert.ToDecimal(row["Số tiền TU"]);
                                ptu_.ghiChu = row["Lý do"].ToString();
                                ptu_.NhanVien = new NhanVienModel
                                {
                                    hoVaTen = row["Tên nhân viên"].ToString(),
                                };


                                lstPhieuTamUng.Add(ptu_);
                            }

                            //đưa data vào
                            foreach (PhieuTamUng ptu in lstPhieuTamUng.OrderBy(d => d.sTT))
                            {
                                if (!String.IsNullOrEmpty(ptu.soTaiKhoan))
                                {
                                    var getMaNV = linqNS.tbl_NS_NhanViens.Where(d => d.soTaiKhoan.Trim() == ptu.soTaiKhoan.Trim() && d.trangThai == 0).Select(d => d.maNhanVien.Trim()).FirstOrDefault();

                                    if (getMaNV != null && getMaNV != "")
                                    {
                                        //Check if ma nhan vien
                                        var checkMaNV = linqNS.tbl_NS_PhieuTamUngs.Where(d => d.maNhanVien.Trim() == getMaNV &&
                                            d.tamUngNam == ptu.tamUngNam
                                            && d.tamUngThang == ptu.tamUngThang).FirstOrDefault();

                                        if (checkMaNV == null)
                                        {
                                            tbl_NS_PhieuTamUng tamUngImport = new tbl_NS_PhieuTamUng();
                                            tamUngImport.maNhanVien = getMaNV;
                                            tamUngImport.soPhieu = GenerateUtil.CheckLetter("DNTU", GetMax());
                                            tamUngImport.nguoiLap = GetUser().manv;
                                            tamUngImport.ngayLap = DateTime.Now;
                                            //tamUngImport.ngayTamUng = String.IsNullOrEmpty(row["Ngày thu tạm ứng"].ToString()) ? (DateTime?)null : DateTime.Parse(row["Ngày thu tạm ứng"].ToString());
                                            //tamUngImport.tamUngNam = String.IsNullOrEmpty(row["Năm"].ToString()) ? 0 : Convert.ToInt32(row["Năm"]);
                                            //tamUngImport.tamUngThang = String.IsNullOrEmpty(row["Tháng"].ToString()) ? 0 : Convert.ToInt32(row["Tháng"]);
                                            //tamUngImport.soTien = String.IsNullOrEmpty(row["Số tiền TU"].ToString()) ? 0 : Convert.ToDouble(row["Số tiền TU"]);
                                            tamUngImport.ngayTamUng = ptu.ngayTamUng;
                                            tamUngImport.tamUngNam = ptu.tamUngNam;
                                            tamUngImport.tamUngThang = ptu.tamUngThang;
                                            tamUngImport.soTien = Convert.ToDouble(ptu.soTien);

                                            tamUngImport.ghiChu = ptu.ghiChu;

                                            linqNS.tbl_NS_PhieuTamUngs.InsertOnSubmit(tamUngImport);

                                            tbl_HT_DMNguoiDuyet nd = new tbl_HT_DMNguoiDuyet();
                                            nd.maCongViec = "PhieuTamUng";
                                            nd.maPhieu = tamUngImport.soPhieu;
                                            nd.maNV = GetUser().manv;
                                            nd.trangThai = 2;
                                            nd.ngayDuyet = DateTime.Now;
                                            linqNS.GetTable<tbl_HT_DMNguoiDuyet>().InsertOnSubmit(nd);
                                            linqNS.SubmitChanges();
                                        }
                                        else
                                        {
                                            // Đã tồn tại nhân viên
                                            // Luu nhan vien khong import duoc
                                            sys_NhanVienImportFailed tblImportFailed = new sys_NhanVienImportFailed();

                                            tblImportFailed.tenNhanVien = ptu.NhanVien.hoVaTen.Trim() + " " + ptu.soTaiKhoan.Trim();
                                            tblImportFailed.lyDoFaild = "Import phiếu tạm ứng không được. Nhân viên này đã thêm tạm ứng. Số phiếu tham chiếu: " + checkMaNV.soPhieu;
                                            tblImportFailed.ngayLap = DateTime.Now;
                                            tblImportFailed.nguoiLap = GetUser().manv;
                                            tblImportFailed.thang = ptu.tamUngThang;
                                            tblImportFailed.nam = ptu.tamUngNam;
                                            tblImportFailed.soTien = ptu.soTien;
                                            tblImportFailed.loaiImport = "PhieuTamUng";
                                            tblImportFailed.tenCongViec = taskIDSystem;
                                            tblImportFailed.maPhieuThamChieu = checkMaNV.soPhieu;
                                            linqNS.sys_NhanVienImportFaileds.InsertOnSubmit(tblImportFailed);
                                            linqNS.SubmitChanges();
                                            //
                                            // Cap nhat lap lai cho phieu cha
                                        }
                                    }

                                }
                                else
                                {
                                    var getMaNV = linqNS.tbl_NS_NhanViens.Where(d => d.ho.Trim() + " " + d.ten.Trim() == ptu.NhanVien.hoVaTen.ToString() && d.trangThai == 0).Select(d => d.maNhanVien).FirstOrDefault();
                                    if (getMaNV != null && getMaNV != "")
                                    {
                                        //Check if ma nhan vien
                                        var checkMaNV = linqNS.tbl_NS_PhieuTamUngs.Where(d => d.maNhanVien == getMaNV && d.tamUngNam == ptu.tamUngNam && d.tamUngThang == ptu.tamUngThang).FirstOrDefault();
                                        if (checkMaNV == null)
                                        {
                                            tbl_NS_PhieuTamUng tamUngImport = new tbl_NS_PhieuTamUng();
                                            tamUngImport.maNhanVien = getMaNV;
                                            tamUngImport.soPhieu = GenerateUtil.CheckLetter("DNTU", GetMax());
                                            tamUngImport.nguoiLap = GetUser().manv;
                                            tamUngImport.ngayLap = DateTime.Now;
                                            tamUngImport.ngayTamUng = ptu.ngayTamUng;
                                            tamUngImport.tamUngNam = ptu.tamUngNam;
                                            tamUngImport.tamUngThang = ptu.tamUngThang;
                                            tamUngImport.soTien = Convert.ToDouble(ptu.soTien);
                                            tamUngImport.ghiChu = ptu.ghiChu;

                                            linqNS.tbl_NS_PhieuTamUngs.InsertOnSubmit(tamUngImport);
                                            linqNS.SubmitChanges();
                                        }
                                        else
                                        {
                                            // Đã tồn tại nhân viên
                                            // Luu nhan vien khong import duoc
                                            sys_NhanVienImportFailed tblImportFailed = new sys_NhanVienImportFailed();
                                            tblImportFailed.tenNhanVien = ptu.NhanVien.hoVaTen.Trim() + " " + ptu.soTaiKhoan.Trim();
                                            tblImportFailed.lyDoFaild = "Import phiếu tạm ứng không được. Nhân viên này đã thêm tạm ứng. Số phiếu tham chiếu: " + checkMaNV.soPhieu;
                                            tblImportFailed.ngayLap = DateTime.Now;
                                            tblImportFailed.nguoiLap = GetUser().manv;
                                            tblImportFailed.thang = ptu.tamUngThang;
                                            tblImportFailed.nam = ptu.tamUngNam;
                                            tblImportFailed.soTien = ptu.soTien;
                                            tblImportFailed.loaiImport = "PhieuTamUng";
                                            tblImportFailed.tenCongViec = taskIDSystem;
                                            tblImportFailed.maPhieuThamChieu = checkMaNV.soPhieu;
                                            linqNS.sys_NhanVienImportFaileds.InsertOnSubmit(tblImportFailed);
                                            linqNS.SubmitChanges();
                                            //
                                            // Cap nhat lap lai cho phieu cha
                                        }
                                    }
                                    else
                                    {
                                        // Khong tim thay so tai khoan, ten nhan vien
                                        // Luu nhan vien khong import duoc
                                        if (ptu.tamUngThang.ToString().Length >= 1)
                                        {
                                            sys_NhanVienImportFailed tblImportFailed = new sys_NhanVienImportFailed();
                                            tblImportFailed.tenNhanVien = ptu.NhanVien.hoVaTen.Trim() + " " + ptu.soTaiKhoan.Trim();
                                            tblImportFailed.lyDoFaild = "Import phiếu tạm ứng không được. Không tìm thấy nhân viên ứng với số tài khoản hoặc tên nhân viên.";
                                            tblImportFailed.ngayLap = DateTime.Now;
                                            tblImportFailed.nguoiLap = GetUser().manv;
                                            tblImportFailed.thang = ptu.tamUngThang;
                                            tblImportFailed.nam = ptu.tamUngThang;
                                            tblImportFailed.soTien = ptu.soTien;
                                            tblImportFailed.loaiImport = "PhieuTamUng";
                                            tblImportFailed.tenCongViec = taskIDSystem;
                                            tblImportFailed.maPhieuThamChieu = "";
                                            linqNS.sys_NhanVienImportFaileds.InsertOnSubmit(tblImportFailed);
                                            linqNS.SubmitChanges();
                                            //
                                            // Cap nhat lap lai cho phieu cha
                                        }
                                    }
                                }


                            }
                        }


                        System.IO.File.Delete(Server.MapPath("/UploadFiles/PhieuTamUng/" + fileName));
                    }
                }
                SaveActiveHistory("Import danh sách phiếu tạm ứng.");
                return Json(new { success = true });
            }
            catch
            {
                return View();
            }
        }
        #region Xuất excel
        public void XuatFile(string qSearch, int thang, int nam, string trangThai)
        {

            var filename = "";
            var virtualPath = HttpRuntime.AppDomainAppVirtualPath;
            var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplatePhieuTamUng.xls"), FileMode.Open, FileAccess.Read);

            var workbook = new HSSFWorkbook(fileStream, true);
            filename += "DanhSachPhieuTamUng.xls";


            var sheet = workbook.GetSheet("danhsachphieutamung");

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

            string rowtitle = "DANH SÁCH PHIẾU TẠM ỨNG";
            var titleCell = HSSFCellUtil.CreateCell(sheet.CreateRow(firstRowNumber), 3, rowtitle.ToUpper());
            titleCell.CellStyle = styleTitle;

            ++firstRowNumber;

            var list1 = new List<string>();
            list1.Add("STT");
            list1.Add("Số phiếu");
            list1.Add("Mã nhân viên");
            list1.Add("Tên nhân viên");
            list1.Add("Tháng");
            list1.Add("Năm");
            list1.Add("Số tiền TU");

            //Start row 13
            var headerRow = sheet.CreateRow(2);
            ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);

            //Create header end
            var idRowStart = 3;
            var datas = linqNS.sp_NS_PhieuTamUng_Index(thang, nam, qSearch, trangThai).OrderBy(d => d.maNhanVien).ToList();
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
                    ReportHelperExcel.SetAlignment(rowC, dem++, item.soPhieu, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item.maNhanVien, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item.hoTenNhanVien, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, Convert.ToString(item.tamUngThang ?? 0), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, Convert.ToString(item.tamUngNam ?? 0), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.###}", item.soTien), hStyleConRight);
                }

                sheet.SetColumnWidth(0, 5 * 250);
                sheet.SetColumnWidth(1, 20 * 270);
                sheet.SetColumnWidth(2, 20 * 270);
                sheet.SetColumnWidth(3, 20 * 270);
                sheet.SetColumnWidth(4, 15 * 200);
                sheet.SetColumnWidth(5, 15 * 200);
                sheet.SetColumnWidth(6, 20 * 270);
            }
            else
            {

                sheet.SetColumnWidth(0, 5 * 250);
                sheet.SetColumnWidth(1, 20 * 270);
                sheet.SetColumnWidth(2, 20 * 270);
                sheet.SetColumnWidth(3, 20 * 270);
                sheet.SetColumnWidth(4, 15 * 200);
                sheet.SetColumnWidth(5, 15 * 200);
                sheet.SetColumnWidth(6, 20 * 270);

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
        #region Xuất excel Phiếu tạm ứng
        public void XuatFileKhongImportDuoc(int thang, int nam)
        {

            var filename = "";
            var virtualPath = HttpRuntime.AppDomainAppVirtualPath;
            var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplatePhieuTamUng.xls"), FileMode.Open, FileAccess.Read);

            var workbook = new HSSFWorkbook(fileStream, true);
            filename += "DanhSachPhieuTamUngKhongImportDuoc.xls";


            var sheet = workbook.GetSheet("danhsachphieutamung");

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

            string rowtitle = "DANH SÁCH PHIẾU TẠM ỨNG KHÔNG IMPORT ĐƯỢC.";
            var titleCell = HSSFCellUtil.CreateCell(sheet.CreateRow(firstRowNumber), 3, rowtitle.ToUpper());
            titleCell.CellStyle = styleTitle;

            ++firstRowNumber;

            var list1 = new List<string>();
            list1.Add("STT");
            list1.Add("Mã nhân viên");
            list1.Add("Tên nhân viên");
            list1.Add("Tháng");
            list1.Add("Năm");
            list1.Add("Số tiền TU");
            list1.Add("Lý do không import được.");
            list1.Add("Mã phiếu tham chiếu(nếu có).");

            //Start row 13
            var headerRow = sheet.CreateRow(2);
            ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);

            //Create header end
            var idRowStart = 3;
            var datas = linqNS.sys_NhanVienImportFaileds.Where(d => d.thang == thang && d.nam == nam && d.loaiImport == taskIDSystem).OrderBy(d => d.tenNhanVien).ToList();
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
                    ReportHelperExcel.SetAlignment(rowC, dem++, item.maNhanVien, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item.tenNhanVien, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, Convert.ToString(item.thang ?? 0), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, Convert.ToString(item.nam ?? 0), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0.###}", item.soTien), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item.lyDoFaild, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item.maPhieuThamChieu, hStyleConLeft);
                }

                sheet.SetColumnWidth(0, 5 * 250);
                sheet.SetColumnWidth(1, 20 * 270);
                sheet.SetColumnWidth(2, 20 * 270);
                sheet.SetColumnWidth(3, 20 * 270);
                sheet.SetColumnWidth(4, 15 * 200);
                sheet.SetColumnWidth(5, 15 * 200);
                sheet.SetColumnWidth(6, 40 * 270);
                sheet.SetColumnWidth(7, 20 * 270);
            }
            else
            {

                sheet.SetColumnWidth(0, 5 * 250);
                sheet.SetColumnWidth(1, 20 * 270);
                sheet.SetColumnWidth(2, 20 * 270);
                sheet.SetColumnWidth(3, 20 * 270);
                sheet.SetColumnWidth(4, 15 * 200);
                sheet.SetColumnWidth(5, 15 * 200);
                sheet.SetColumnWidth(6, 40 * 270);
                sheet.SetColumnWidth(7, 20 * 270);

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
        public ActionResult ChonNhanVien()
        {
            StringBuilder buildTree = new StringBuilder();
            var phongBans = linqDM.tbl_DM_PhongBans.ToList();
            buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
            ViewBag.NVPB = buildTree.ToString();
            return PartialView("_ChonNhanVien");
        }
        public ActionResult LoadNhanVien(int? page, string searchString, string maPhongBan)
        {
            IList<sp_PB_DanhSachNhanVienResult> phongBan1s;
            phongBan1s = linqDM.sp_PB_DanhSachNhanVien(searchString, maPhongBan).ToList();
            ViewBag.isGet = "True";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            ViewBag.Count = phongBan1s.Count();
            ViewBag.Search = searchString;
            ViewBag.MaPhongBan = maPhongBan;
            return PartialView("_LoadNhanVien", phongBan1s.ToPagedList(currentPageIndex, 10));
        }

        public void buitlLoaiThuNhapKhac(string select)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            var loaiThuNhapKhac = linqDM.tbl_DM_ThuNhapKhacs.ToList();

            dict.Add("", "[Chọn loại thu nhập]");
            foreach (var item in loaiThuNhapKhac)
            {
                dict.Add(item.maLoaiThuNhapKhac.ToString(), item.tenLoaiThuNhapKhac);
            }
            ViewBag.loaiThuNhapKhac = new SelectList(dict, "Key", "Value", select);

        }
        public void builtThang(int thang)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            for (int i = 1; i <= 12; i++)
            {
                dict.Add(i, i);
            }
            ViewBag.Thangs = new SelectList(dict, "Key", "Value", thang);
        }
        public void builtNam(int nam)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            for (int i = DateTime.Now.Year - 5; i <= DateTime.Now.Year + 5; i++)
            {
                dict.Add(i, i);
            }
            ViewBag.Nams = new SelectList(dict, "Key", "Value", nam);
        }
        public string HoVaTen(string MaNV)
        {

            return linqNS.tbl_NS_NhanViens.Where(d => d.maNhanVien == MaNV).Select(d => d.ho + " " + d.ten).FirstOrDefault();
        }
        public ActionResult ChonPhongBan()
        {
            StringBuilder buildTree = new StringBuilder();
            IList<tbl_DM_PhongBan> phongBans = linqDM.tbl_DM_PhongBans.ToList();
            buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
            ViewBag.PhongBan = buildTree.ToString();
            return PartialView("_ChonPhongBan");
        }

        public ActionResult XacNhan(string id)
        {

            return Json(string.Empty);
        }

        #region Duyệt qui trình động
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


        /// <summary>
        /// Duyệt theo qui trình động phiếu nghỉ phép
        /// </summary>
        /// <param name="maPhieu"></param>
        /// <param name="maQuiTrinh"></param>
        /// <param name="sendMail"></param>
        /// <param name="sendSMS"></param>
        /// <returns></returns>
        public ActionResult DuyetTheoQuiTrinhDong(string maPhieu, string maCongViec, bool sendMail, bool sendSMS, string maNhanVien, int soNgayNghiPhep, string lyDo, int idQuiTrinh)
        {
            try
            {
                LinqHeThongDataContext ht = new LinqHeThongDataContext();
                string hoTen = (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ho).FirstOrDefault() ?? string.Empty) + " " + (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ten).FirstOrDefault() ?? string.Empty);
                DMNguoiDuyetController _nguoiDuyet = new DMNguoiDuyetController();
                var kq = _nguoiDuyet.DuyeTheoQuiTrinhDong(maPhieu, maCongViec, sendMail, sendSMS, GetUser().manv, hoTen, soNgayNghiPhep, lyDo, idQuiTrinh, string.Empty);
                return kq;
            }
            catch (Exception e)
            {
                return Json("Lỗi: " + e.Message);
            }
        }
        #endregion

    }
}
