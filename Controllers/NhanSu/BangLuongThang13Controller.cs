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
using System.Data;

namespace BatDongSan.Controllers.NhanSu
{
    public class BangLuongThang13Controller : ApplicationController
    {
        LinqPhieuDeNghiDataContext lqPhieuDN = new LinqPhieuDeNghiDataContext();
        LinqNhanSuDataContext linqNS = new LinqNhanSuDataContext();
        LinqDanhMucDataContext linqDM = new LinqDanhMucDataContext();
        tbl_NS_BangLuongThang13 tblBangLuongThang13;
        IList<tbl_NS_BangLuongThang13> tblBangLuongThang13s;
        BangLuongThang13 PhieuDeNghiModel;
        public const string taskIDSystem = "BangLuongThang13";//REGWORKVOTE
        public bool? permission;
        //
        // GET: /PhieuCongTac/

        public ActionResult Index()
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
            

            ViewBag.isGet = "True";
            nam(DateTime.Now.Year);
            return View();

        }
        public ActionResult ViewIndex(string qSearch, int nam, int _page = 0)
        {
            
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            string maNhanVien = GetUser().manv;
            int page = _page == 0 ? 1 : _page;
            int pIndex = page;
            int checkDuyet = 0;
            var Duyets = linqNS.tbl_DuyetLuongThang13s.Where(d => d.nam == nam).FirstOrDefault();
            if (Duyets != null) { checkDuyet = 1; }
            ViewBag.statusCheckDuyet = checkDuyet;
            int total = linqNS.sp_NS_BangLuongThang13_Index(nam, qSearch).Count();
            PagingLoaderController("/BangLuongThang13/Index/", total, page, "?qsearch=" + qSearch + "&nam=" + nam);
            ViewData["lsDanhSach"] = linqNS.sp_NS_BangLuongThang13_Index(nam, qSearch).Skip(start).Take(offset).ToList();

            ViewData["qSearch"] = qSearch;
            return PartialView("ViewIndex");
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

            PhieuDeNghiModel = new BangLuongThang13();
            PhieuDeNghiModel.maPhieu = GenerateUtil.CheckLetter("BLT13", GetMax());
            PhieuDeNghiModel.ngayLap = DateTime.Now;
            
            PhieuDeNghiModel.soTien = 0;
            builtNam(DateTime.Now.Year);

            return View(PhieuDeNghiModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(FormCollection coll)
        {
            try
            {
                tblBangLuongThang13 = new tbl_NS_BangLuongThang13();
                tblBangLuongThang13.soPhieu = GenerateUtil.CheckLetter("BLT13", GetMax());
                tblBangLuongThang13.nguoiLap = GetUser().manv;
                tblBangLuongThang13.ngayLap = DateTime.Now;
                tblBangLuongThang13.maNhanVien = coll.Get("maNhanVien");
                tblBangLuongThang13.nam = Convert.ToInt32(coll.Get("nam"));
                tblBangLuongThang13.luongThang13 = Convert.ToDouble(coll.Get("soTien"));
                tblBangLuongThang13.ghiChu = coll.Get("ghiChu");
                linqNS.tbl_NS_BangLuongThang13s.InsertOnSubmit(tblBangLuongThang13);
                linqNS.SubmitChanges();

                return RedirectToAction("Edit", new { id = tblBangLuongThang13.soPhieu });
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }
        public string GetMax()
        {
            return linqNS.tbl_NS_BangLuongThang13s.OrderByDescending(d=>d.ngayLap).Select(d => d.soPhieu).FirstOrDefault() ?? string.Empty;
        }
        [HttpPost]
        public int Delete(string maPhieu)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenXoa);
            if (!permission.HasValue)
                return 0;
            if (!permission.Value)
                return 0;
            #endregion
            try
            {
                var phieu = linqNS.tbl_NS_BangLuongThang13s.Where(d => d.soPhieu == maPhieu).FirstOrDefault();
                linqNS.tbl_NS_BangLuongThang13s.DeleteOnSubmit(phieu);
                linqNS.SubmitChanges();
                return 1;
            }
            catch
            {
                return 0;
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

            var dataPhieuCongTac = linqNS.tbl_NS_BangLuongThang13s.Where(d => d.soPhieu == id).FirstOrDefault();
            PhieuDeNghiModel = new BangLuongThang13();
            PhieuDeNghiModel.maPhieu = dataPhieuCongTac.soPhieu;
            PhieuDeNghiModel.maNhanVien = dataPhieuCongTac.maNhanVien;
            PhieuDeNghiModel.hoTen = linqNS.vw_NS_DanhSachNhanVienTheoPhongBans.Where(d => d.maNhanVien == dataPhieuCongTac.maNhanVien).Select(d => d.ho + " " + d.ten).FirstOrDefault();
            PhieuDeNghiModel.hoTenNguoiLap = linqNS.vw_NS_DanhSachNhanVienTheoPhongBans.Where(d => d.maNhanVien == dataPhieuCongTac.nguoiLap).Select(d => d.ho + " " + d.ten).FirstOrDefault();
            PhieuDeNghiModel.ngayLap = dataPhieuCongTac.ngayLap;
            PhieuDeNghiModel.nguoiLap = dataPhieuCongTac.nguoiLap;
            PhieuDeNghiModel.soTien = (decimal?)dataPhieuCongTac.luongThang13;
            PhieuDeNghiModel.ghiChu = dataPhieuCongTac.ghiChu;
            PhieuDeNghiModel.nam = dataPhieuCongTac.nam;
            builtNam(dataPhieuCongTac.nam);
            return View(PhieuDeNghiModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(string id, FormCollection coll)
        {
            try
            {
                tblBangLuongThang13 = linqNS.tbl_NS_BangLuongThang13s.Where(d => d.soPhieu == id).FirstOrDefault();
                tblBangLuongThang13.nam = Convert.ToInt32(coll.Get("nam"));
                tblBangLuongThang13.luongThang13 = Convert.ToDouble(coll.Get("soTien"));
                tblBangLuongThang13.ghiChu = coll.Get("ghiChu");
                linqNS.SubmitChanges();

                return RedirectToAction("Edit", new { id = tblBangLuongThang13.soPhieu });
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
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            var dataPhieuCongTac = linqNS.tbl_NS_BangLuongThang13s.Where(d => d.soPhieu == id).FirstOrDefault();
            PhieuDeNghiModel = new BangLuongThang13();
            PhieuDeNghiModel.maPhieu = dataPhieuCongTac.soPhieu;
            PhieuDeNghiModel.maNhanVien = dataPhieuCongTac.maNhanVien;
            PhieuDeNghiModel.hoTen = linqNS.vw_NS_DanhSachNhanVienTheoPhongBans.Where(d => d.maNhanVien == dataPhieuCongTac.maNhanVien).Select(d => d.ho + " " + d.ten).FirstOrDefault();
            PhieuDeNghiModel.hoTenNguoiLap = linqNS.vw_NS_DanhSachNhanVienTheoPhongBans.Where(d => d.maNhanVien == dataPhieuCongTac.nguoiLap).Select(d => d.ho + " " + d.ten).FirstOrDefault();
            PhieuDeNghiModel.ngayLap = dataPhieuCongTac.ngayLap;
            PhieuDeNghiModel.nguoiLap = dataPhieuCongTac.nguoiLap;
            PhieuDeNghiModel.soTien = (decimal?)dataPhieuCongTac.luongThang13;
            PhieuDeNghiModel.ghiChu = dataPhieuCongTac.ghiChu;
            PhieuDeNghiModel.nam = dataPhieuCongTac.nam;
            builtNam(dataPhieuCongTac.nam);
            return View(PhieuDeNghiModel);
        }
        public ActionResult DuyetLuongThang13(int nam)
        {


            try
            {
                #region Role user
                permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenDuyet);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion
                //Check 
                var result = new { kq = false };
                var checkEx = linqNS.tbl_DuyetLuongThang13s.Where(t => t.nam == nam).FirstOrDefault();
                if (checkEx != null)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                //End check
                // Insert Row 
                tbl_DuyetLuongThang13 tblDuyetBL = new tbl_DuyetLuongThang13();
                tblDuyetBL.nam = nam;
                tblDuyetBL.ngayDuyet = DateTime.Now;
                tblDuyetBL.nguoiDuyet = GetUser().manv;
                linqNS.tbl_DuyetLuongThang13s.InsertOnSubmit(tblDuyetBL);
                linqNS.SubmitChanges();
                // End Insert Row
                // Check Exist
                var list = linqNS.tbl_DuyetLuongThang13s.Where(t => t.nam == nam).FirstOrDefault();

                if (list != null)
                {
                    result = new { kq = true };
                }
                SaveActiveHistory("Duyệt lương tháng 13, năm: " + nam);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return View();
            }

        }
        public int CheckMaNhanVienNam(string maNhanVien, int nam)
        {
            //BatDongSan.Models.ChamCong.LinqChamCongServerDataContext contextCC = new BatDongSan.Models.ChamCong.LinqChamCongServerDataContext();
            var checkList =linqNS.tbl_NS_BangLuongThang13s.Where(d => d.maNhanVien == maNhanVien && d.nam == nam).FirstOrDefault();
            if (checkList != null)
            {
                return 1;
            }
           
            return 0;
        }
        public FileResult DownloadImportFile()
        {
            string savedFileName = Path.Combine("/UploadFiles/Template/", "BangLuongThang13.xlsx");
            return File(savedFileName, "multipart/form-data", "BangLuongThang13.xlsx");
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
                        string savedLocation = "/UploadFiles/BangLuongThang13/";
                        Directory.CreateDirectory(savedLocation);
                        var filePath = Server.MapPath(savedLocation);
                        string fileName = date.ToString() + File.FileName;
                        string savedFileName = Path.Combine(filePath, fileName);
                        File.SaveAs(savedFileName);

                        ExcelDataProcessing excelDataProcessor = new ExcelDataProcessing(savedFileName);
                        DataTable dt = excelDataProcessor.GetDataTableWorkSheet("BangLuongThang13");
                       // IList<tbl_NS_BangLuongThang13> tamUngImports= new List<tbl_NS_BangLuongThang13>();
                       
                        foreach (DataRow row in dt.Rows)
                        {
                            if (String.IsNullOrEmpty(row["Mã nhân viên"].ToString()))
                                break;
                            //Check if ma nhan vien
                            var checkMaNV = linqNS.tbl_NS_BangLuongThang13s.Where(d => d.maNhanVien == row["Mã nhân viên"].ToString() && d.nam == Convert.ToInt32(row["Năm"])).FirstOrDefault();
                            if (checkMaNV == null)
                            {
                                tbl_NS_BangLuongThang13 tamUngImport = new tbl_NS_BangLuongThang13();
                                tamUngImport.maNhanVien = row["Mã nhân viên"].ToString();
                                tamUngImport.soPhieu = GenerateUtil.CheckLetter("BLT13", GetMax());
                                tamUngImport.nguoiLap = GetUser().manv;
                                tamUngImport.ngayLap = DateTime.Now;
                                tamUngImport.nam = String.IsNullOrEmpty(row["Năm"].ToString()) ? 0 : Convert.ToInt32(row["Năm"]);
                                tamUngImport.luongThang13 = String.IsNullOrEmpty(row["Lương tháng 13"].ToString()) ? 0 : Convert.ToDouble(row["Lương tháng 13"]);
                                tamUngImport.ghiChu = row["Ghi chú"].ToString();
                                
                                linqNS.tbl_NS_BangLuongThang13s.InsertOnSubmit(tamUngImport);
                                linqNS.SubmitChanges();
                            }
                          
                        }

                        System.IO.File.Delete(Server.MapPath("/UploadFiles/BangLuongThang13/" + fileName));
                    }
                }
                SaveActiveHistory("Import danh sách lương tháng 13");
                return Json(new { success = true });
            }
            catch
            {
                return View();
            }
        }

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
        private void thang(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 1; i < 13; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["thang"] = new SelectList(dics, "Key", "Value", value);
            ViewData["thangtc"] = new SelectList(dics, "Key", "Value", value);
        }
        private void nam(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = (DateTime.Now.Year - 5); i < (DateTime.Now.Year + 10); i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["nam"] = new SelectList(dics, "Key", "Value", value);
            ViewData["namtc"] = new SelectList(dics, "Key", "Value", value);
        }
        public ActionResult XacNhan(string id)
        {

            return Json(string.Empty);
        }

        

    }
}
