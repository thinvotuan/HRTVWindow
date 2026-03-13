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
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.HSSF.UserModel.Contrib;
using Worldsoft.Mvc.Web.Util;
using NPOI.SS.Util;
using BatDongSan.Models.DBChamCong;
namespace BatDongSan.Controllers.TinhCong
{
    public class BangChamCongTongHopController : ApplicationController
    {

        private LinqNhanSuDataContext nhanSuContext = new LinqNhanSuDataContext();
        private IList<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan> phongBans;
        private StringBuilder buildTree;
        private readonly string MCV = "ChamCongAM";
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
            thang(DateTime.Now.Month);
            nam(DateTime.Now.Year);
            return View("");

        }
        public ActionResult XemBangChamCongChiTiet()
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            //  DoDuLieuVanTay();

            buildTree = new StringBuilder();
            phongBans = nhanSuContext.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan>().ToList();
            buildTree = TreePhongBanAjax.BuildTreeDepartment(phongBans);
            ViewBag.PhongBans = buildTree.ToString();
            thang(DateTime.Now.Month);
            nam(DateTime.Now.Year);
            TuNgay(1);
            DenNgay(10);
            Administrator(GetUser().manv);
            return View("");
        }
        public ActionResult LoadBangChamCongChiTiet(string qSearch, int thang, int nam, int _page = 0)
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
            int total = nhanSuContext.sp_NS_BangChamCongChiTiet(thang, nam, qSearch, "").Count();
            PagingLoaderController("/BangChamCongTongHop/LoadBangChamCongChiTiet/", total, page, "?qsearch=" + qSearch);
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangChamCongChiTiet(thang, nam, qSearch, "").Skip(start).Take(offset).ToList();

            ViewData["qSearch"] = qSearch;
            return PartialView("_LoadBangChamCongChiTiet");
        }

        #region Xuat File Bang cham cong tong hop
        public void XuatFileBangChamCongTH(int thang, int nam, string qSearch)
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

                var soNgay = DateTime.DaysInMonth(nam, thang);
                var filename = "";
                var virtualPath = HttpRuntime.AppDomainAppVirtualPath;

                var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplatecc.xls"), FileMode.Open, FileAccess.Read);

                var workbook = new HSSFWorkbook(fileStream, true);
                filename += "BangChamCongTongHop_" + nam + "_" + thang + ".xls";

                #region format style excel cell
                /*style title start*/
                //tạo font cho các title
                //font tiêu đề 
                HSSFFont hFontTieuDe = (HSSFFont)workbook.CreateFont();
                hFontTieuDe.FontHeightInPoints = 18;
                hFontTieuDe.Boldweight = 100 * 10;
                hFontTieuDe.FontName = "Times New Roman";
                hFontTieuDe.Color = HSSFColor.BLACK.index;
                //font tiêu đề Yellow
                HSSFFont hFontTieuDeYellow = (HSSFFont)workbook.CreateFont();
                hFontTieuDeYellow.FontHeightInPoints = 18;
                hFontTieuDeYellow.Boldweight = 100 * 10;
                hFontTieuDeYellow.FontName = "Times New Roman";
                hFontTieuDeYellow.Color = HSSFColor.BLACK.index;

                //font chữ bình thường
                HSSFFont hFontNommal = (HSSFFont)workbook.CreateFont();
                hFontNommal.Color = HSSFColor.BLACK.index;
                hFontNommal.FontName = "Times New Roman";

                //font Cty
                HSSFFont hFontNommalCTY = (HSSFFont)workbook.CreateFont();
                hFontNommalCTY.FontHeightInPoints = 18;
                hFontNommalCTY.Boldweight = 100 * 10;
                hFontNommalCTY.FontName = "Times New Roman";
                hFontNommalCTY.Color = HSSFColor.BLACK.index;

                HSSFFont hFontNommalTieuDe = (HSSFFont)workbook.CreateFont();
                hFontNommalTieuDe.FontHeightInPoints = 18;
                hFontNommalTieuDe.FontName = "Times New Roman";
                hFontNommalTieuDe.Color = HSSFColor.BLACK.index;

                //font tiêu đề 
                HSSFFont hFontTongGiaTriHT = (HSSFFont)workbook.CreateFont();
                hFontTongGiaTriHT.FontHeightInPoints = 11;
                hFontTongGiaTriHT.Boldweight = (short)FontBoldWeight.BOLD;
                hFontTongGiaTriHT.FontName = "Times New Roman";
                hFontTongGiaTriHT.Color = HSSFColor.BLACK.index;

                //font thông tin bảng tính
                HSSFFont hFontTT = (HSSFFont)workbook.CreateFont();
                hFontTT.IsItalic = true;
                hFontTT.Boldweight = (short)FontBoldWeight.NORMAL;
                hFontTT.Color = HSSFColor.BLACK.index;
                hFontTT.FontName = "Times New Roman";
                hFontTieuDe.FontHeightInPoints = 11;
                HSSFFont hFontTT13 = (HSSFFont)workbook.CreateFont();
                hFontTT13.IsItalic = false;
                hFontTT13.Boldweight = (short)FontBoldWeight.BOLD;
                hFontTT13.Color = HSSFColor.BLACK.index;
                hFontTT13.FontName = "Times New Roman";
                hFontTT13.FontHeightInPoints = 10;
                HSSFFont hFontTT26 = (HSSFFont)workbook.CreateFont();
                hFontTT26.IsItalic = false;
                hFontTT26.Boldweight = (short)FontBoldWeight.NORMAL;
                hFontTT26.Color = HSSFColor.BLACK.index;
                hFontTT26.FontName = "Times New Roman";
                hFontTT26.FontHeightInPoints = 12;

                //font chứ hoa đậm
                HSSFFont hFontNommalUpper = (HSSFFont)workbook.CreateFont();
                hFontNommalUpper.Boldweight = (short)FontBoldWeight.BOLD;
                hFontNommalUpper.Color = HSSFColor.BLACK.index;
                hFontNommalUpper.FontName = "Times New Roman";



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
                var styleTitleYellow = workbook.CreateCellStyle();
                styleTitleYellow.SetFont(hFontTieuDe);
                styleTitleYellow.Alignment = HorizontalAlignment.JUSTIFY;
                styleTitleYellow.FillBackgroundColor = HSSFColor.YELLOW.index;
                styleTitleYellow.FillForegroundColor = HSSFColor.YELLOW.index;
                styleTitleYellow.FillPattern = FillPatternType.SOLID_FOREGROUND;
                var styleTitle2 = workbook.CreateCellStyle();
                styleTitle2.SetFont(hFontTT);
                styleTitle2.Alignment = HorizontalAlignment.LEFT;
                var styleTitle13 = workbook.CreateCellStyle();
                styleTitle13.SetFont(hFontTT13);
                styleTitle13.Alignment = HorizontalAlignment.LEFT;
                var styleTitle26 = workbook.CreateCellStyle();
                styleTitle26.SetFont(hFontTT26);
                styleTitle26.Alignment = HorizontalAlignment.LEFT;


                var styleTitleCTY = workbook.CreateCellStyle();
                styleTitleCTY.SetFont(hFontNommalCTY);
                styleTitleCTY.Alignment = HorizontalAlignment.LEFT;
                var styleTitleTieuDe = workbook.CreateCellStyle();
                styleTitleTieuDe.SetFont(hFontNommalTieuDe);
                styleTitleTieuDe.Alignment = HorizontalAlignment.LEFT;

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



                var hStyleConLeftBack = (HSSFCellStyle)workbook.CreateCellStyle();
                hStyleConLeftBack.SetFont(hFontNommal);
                hStyleConLeftBack.VerticalAlignment = VerticalAlignment.TOP;
                hStyleConLeftBack.Alignment = HorizontalAlignment.LEFT;
                hStyleConLeftBack.WrapText = true;
                hStyleConLeftBack.BorderBottom = CellBorderType.THIN;
                hStyleConLeftBack.BorderLeft = CellBorderType.THIN;
                hStyleConLeftBack.BorderRight = CellBorderType.THIN;
                hStyleConLeftBack.BorderTop = CellBorderType.THIN;
                hStyleConLeftBack.FillBackgroundColor = HSSFColor.GREY_25_PERCENT.index;
                hStyleConLeftBack.FillForegroundColor = HSSFColor.GREY_25_PERCENT.index;
                hStyleConLeftBack.FillPattern = FillPatternType.SOLID_FOREGROUND;

                var hStyleConCenterBack = (HSSFCellStyle)workbook.CreateCellStyle();
                hStyleConCenterBack.SetFont(hFontNommal);
                hStyleConCenterBack.VerticalAlignment = VerticalAlignment.TOP;
                hStyleConCenterBack.Alignment = HorizontalAlignment.CENTER;
                hStyleConCenterBack.WrapText = true;
                hStyleConCenterBack.BorderBottom = CellBorderType.THIN;
                hStyleConCenterBack.BorderLeft = CellBorderType.THIN;
                hStyleConCenterBack.BorderRight = CellBorderType.THIN;
                hStyleConCenterBack.BorderTop = CellBorderType.THIN;
                hStyleConCenterBack.FillBackgroundColor = HSSFColor.GREY_25_PERCENT.index;
                hStyleConCenterBack.FillForegroundColor = HSSFColor.GREY_25_PERCENT.index;
                hStyleConCenterBack.FillPattern = FillPatternType.SOLID_FOREGROUND;

                var hStyleConCenterYellowBack = (HSSFCellStyle)workbook.CreateCellStyle();
                hStyleConCenterYellowBack.SetFont(hFontNommal);
                hStyleConCenterYellowBack.VerticalAlignment = VerticalAlignment.TOP;
                hStyleConCenterYellowBack.Alignment = HorizontalAlignment.CENTER;
                hStyleConCenterYellowBack.WrapText = true;
                hStyleConCenterYellowBack.BorderBottom = CellBorderType.THIN;
                hStyleConCenterYellowBack.BorderLeft = CellBorderType.THIN;
                hStyleConCenterYellowBack.BorderRight = CellBorderType.THIN;
                hStyleConCenterYellowBack.BorderTop = CellBorderType.THIN;
                hStyleConCenterYellowBack.FillBackgroundColor = HSSFColor.YELLOW.index;
                hStyleConCenterYellowBack.FillForegroundColor = HSSFColor.YELLOW.index;
                hStyleConCenterYellowBack.FillPattern = FillPatternType.SOLID_FOREGROUND;


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

                //Khai báo row
                Row rowC = null;


                string cellTitleMain = "BẢNG CHẤM CÔNG THÁNG " + thang + "/" + nam;
                var sheet = workbook.CreateSheet("thang_" + thang + "_nam_" + nam);
                workbook.ActiveSheetIndex = 1;


                //Khai báo row đầu tiên
                int firstRowNumber = 0;

                string cellTenCty = "TV Window";
                //var titleCellCty = HSSFCellUtil.CreateCell(sheet.CreateRow(1), 0, cellTenCty.ToUpper());
                //titleCellCty.CellStyle = styleTitle;




                string nguoiChamCong = "Phụ trách đơn vị";
                string phuTrachDonVi = "Người theo dõi chấm công";
                string ngayThangNam = "Ngày   tháng   năm " + nam;
                string nguoiDuyetCong = "Người duyệt công";
                //var titleCellTitleMain = HSSFCellUtil.CreateCell(sheet.CreateRow(2), 5, cellTitleMain.ToUpper());
                //titleCellTitleMain.CellStyle = styleTitle;
                var ListTH = nhanSuContext.sp_NS_BangChamCongChiTiet_PhongBan(thang, nam, qSearch, null).ToList();
                firstRowNumber = 0;
                var idRowStart = firstRowNumber;
                BatDongSan.Models.DanhMuc.LinqDanhMucDataContext context = new BatDongSan.Models.DanhMuc.LinqDanhMucDataContext();
                var DMNghiLe = context.tbl_DM_NghiLes.Select(d => d.ngayNghiLe).ToList();

                var countNV = 0;
                if (ListTH.Count > 0)
                {
                    // Khoi tao nhan vien/
                    var MaNVBanDau = ListTH[0].maNhanVien;
                    var thuTuParentNhanVien = 0;
                    var FlashNhanVien = 0;
                    // end khoi tao nhan vien
                    int j = 0;
                    var HSMTBanDau = ListTH[0].maPhongBan;
                    var thuTuParent = 0;
                    var Flash = 0;
                    var FlashFooter = 0;
                    var maPb = 0;
                    var flasRow = 0;
                    var demSTT = 0;
                    var flasRowFooter = 0;
                    var thuTuParentFooter = 0;
                    var demNVPhongBan = 0;
                    var grpNhanVien = 0;
                    foreach (var item in ListTH)
                    {

                        if (HSMTBanDau == item.maPhongBan)
                        {
                            Flash = 0;
                            FlashFooter = 0;
                        }
                        else
                        {
                            HSMTBanDau = item.maPhongBan;
                            Flash = 1;
                            FlashFooter = 1;
                            j = 1;
                        }
                        if (Flash == 1 || thuTuParent == 0)
                        {
                            grpNhanVien = ListTH.Where(d => d.maPhongBan == item.maPhongBan).ToList().Count();
                            demNVPhongBan = 0;
                            if (flasRow == 1)
                            {
                                firstRowNumber = firstRowNumber + 8;
                            }
                            flasRow = 1;
                            thuTuParent = 1;
                            countNV = 0;
                            Flash = 0;
                            idRowStart = firstRowNumber + 2;
                            var headerRowCty = sheet.CreateRow(idRowStart);
                            idRowStart++;
                            var headerRowTieuDe = sheet.CreateRow(idRowStart);
                            idRowStart++;
                            var headerRow0 = sheet.CreateRow(idRowStart);
                            int rowend = idRowStart;
                            maPb = maPb + 1;
                            string cellTitleMainCTY = Convert.ToString(cellTenCty);
                            var titleCellTitleMainCTY = HSSFCellUtil.CreateCell(headerRowCty, 1, cellTitleMainCTY.ToUpper());
                            titleCellTitleMainCTY.CellStyle = styleTitle;
                            string cellTitleMainTieuDe = Convert.ToString(cellTitleMain);
                            var titleCellTitleMainTieuDe = HSSFCellUtil.CreateCell(headerRowTieuDe, 5, cellTitleMainTieuDe.ToUpper());
                            titleCellTitleMainTieuDe.CellStyle = styleTitle;
                            var tenPB = maPb + ". " + item.tenPhongBan;

                            string cellTitleMain2 = Convert.ToString(tenPB);
                            var titleCellTitleMain2 = HSSFCellUtil.CreateCell(headerRow0, 0, cellTitleMain2.ToUpper());
                            titleCellTitleMain2.CellStyle = styleTitleYellow;
                            sheet.AddMergedRegion(new CellRangeAddress(headerRow0.RowNum, headerRow0.RowNum, 0, 10));
                            sheet.SetColumnWidth(headerRow0.RowNum, 15 * 510);
                            idRowStart++;
                            var headerRow21 = sheet.CreateRow(idRowStart + 1);
                            var list1 = new List<string>();
                            list1.Add("STT");
                            list1.Add("Họ và tên");
                            list1.Add("Ngày công trong tháng");
                            for (var i = 2; i <= soNgay; i++)
                            {
                                list1.Add("");

                            }
                            var ngayGop = soNgay;

                            list1.Add("Công chuẩn");
                            list1.Add("Số ngày quét");
                            list1.Add("Công tác");
                            list1.Add("Nghỉ phép năm");
                            list1.Add("Nghỉ bù");
                            list1.Add("Nghỉ khác hưởng lương");
                            list1.Add("Nghỉ phép không lương");
                            list1.Add("Nghỉ lễ");
                            list1.Add("Tăng ca thường");
                            list1.Add("Tăng ca Chủ nhật");
                            list1.Add("Lũy kế tháng trước");
                            list1.Add("Tổng cộng");
                            var list2 = new List<string>();
                            list2.Add("STT");
                            list2.Add("Họ và tên");
                            //list2.Add("Ngày công trong tháng");
                            for (var i = 1; i <= soNgay; i++)
                            {
                                list2.Add(Convert.ToString(i));




                            }

                            list2.Add("Công chuẩn");
                            list2.Add("Số ngày quét");
                            list2.Add("Công tác");
                            list2.Add("Nghỉ phép năm");
                            list2.Add("Nghỉ bù");
                            list2.Add("Nghỉ khác hưởng lương");
                            list2.Add("Nghỉ phép không lương");
                            list2.Add("Nghỉ lễ");
                            list2.Add("Tăng ca thường");
                            list2.Add("Tăng ca Chủ nhật");
                            list2.Add("Lũy kế tháng trước");
                            list2.Add("Tổng cộng");



                            var headerRow = sheet.CreateRow(idRowStart);
                            ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);
                            //var titleTC2 = HSSFCellUtil.GetCell(headerRow, ngayGop + 2);
                            //titleTC2.CellStyle = styleTitleYellow;
                            //var titleTC3 = HSSFCellUtil.GetCell(headerRow, ngayGop + 3);
                            //titleTC3.CellStyle = styleTitleYellow;
                            //var titleTC4 = HSSFCellUtil.GetCell(headerRow, ngayGop + 4);
                            //titleTC4.CellStyle = styleTitleYellow;
                            idRowStart++;
                            var headerRow1 = sheet.CreateRow(idRowStart);
                            ReportHelperExcel.CreateHeaderRow(headerRow1, 0, styleheadedColumnTable, list2);
                            sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, soNgay + 13, soNgay + 13));
                            sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, soNgay + 12, soNgay + 12));
                            sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, soNgay + 11, soNgay + 11));
                            sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, soNgay + 10, soNgay + 10));
                            sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, soNgay + 9, soNgay + 9));
                            sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, soNgay + 8, soNgay + 8));
                            sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, soNgay + 7, soNgay + 7));
                            sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, soNgay + 6, soNgay + 6));
                            sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, soNgay + 5, soNgay + 5));
                            sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, soNgay + 4, soNgay + 4));
                            sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, soNgay + 3, soNgay + 3));
                            sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, soNgay + 2, soNgay + 2));

                            sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 2, (soNgay + 1)));
                            sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 1, 1));
                            sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 0, 0));



                            sheet.SetColumnWidth(0, 5 * 210);
                            sheet.SetColumnWidth(1, 30 * 210);
                            for (var i = 2; i <= soNgay + 1; i++)
                            {
                                sheet.SetColumnWidth(i, 10 * 110);


                            }


                            sheet.SetColumnWidth(soNgay + 2, 15 * 210);
                            sheet.SetColumnWidth(soNgay + 3, 15 * 210);
                            sheet.SetColumnWidth(soNgay + 4, 15 * 210);
                            sheet.SetColumnWidth(soNgay + 5, 15 * 210);
                            sheet.SetColumnWidth(soNgay + 5, 15 * 210);
                            sheet.SetColumnWidth(soNgay + 6, 15 * 210);
                            sheet.SetColumnWidth(soNgay + 7, 15 * 210);
                            sheet.SetColumnWidth(soNgay + 8, 15 * 210);
                            sheet.SetColumnWidth(soNgay + 9, 15 * 210);
                            sheet.SetColumnWidth(soNgay + 10, 15 * 210);
                            sheet.SetColumnWidth(soNgay + 11, 15 * 210);
                            sheet.SetColumnWidth(soNgay + 12, 15 * 210);
                            sheet.SetColumnWidth(soNgay + 13, 15 * 210);


                        }
                        if (Flash == 0)
                        {
                            demNVPhongBan++;
                            // Khoi tao nhan vien/
                            if (MaNVBanDau == item.maNhanVien)
                            {
                                FlashNhanVien = 0;
                            }
                            else
                            {
                                MaNVBanDau = item.maNhanVien;
                                FlashNhanVien = 1;

                            }
                            if (FlashNhanVien == 1 || thuTuParentNhanVien == 0)
                            {

                                countNV++;
                                firstRowNumber++;
                                thuTuParentNhanVien = 1;
                                FlashNhanVien = 0;
                                // end khoi tao nhan vien

                                var stt = 0;
                                int dem = 0;
                                double? sumLuongLe = 0;


                                dem = 0;

                                stt++;
                                idRowStart++;

                                rowC = sheet.CreateRow(idRowStart);
                                ReportHelperExcel.SetAlignment(rowC, dem++, countNV.ToString(), hStyleConCenter);
                                ReportHelperExcel.SetAlignment(rowC, dem++, item.hoTen, hStyleConLeft);
                                // For thu trong tuan cho tung nhan vien
                                var listNgayCongNV = ListTH.Where(d => d.maNhanVien == item.maNhanVien).ToList();
                                for (var ngay = 1; ngay <= soNgay; ngay++)
                                {
                                    int ngayLe = 0;

                                    // Check ngay le
                                    foreach (var itemNgay in DMNghiLe)
                                    {
                                        if (ngay == itemNgay.Value.Date.Day && thang == itemNgay.Value.Date.Month && nam == itemNgay.Value.Date.Year)
                                        {
                                            ngayLe = 1;
                                        }
                                    }



                                    if (ngayLe == 1)
                                    {
                                        // if la ngay le thi ko can check cong
                                        ReportHelperExcel.SetAlignment(rowC, dem++, "L", hStyleConCenterBack);
                                    }
                                    else
                                    {
                                        // Check ngay trong thang co di lam khong
                                        var ngayTrongThang = listNgayCongNV.Where(d => d.ngayTrongThang == ngay).FirstOrDefault();

                                        if (ngayTrongThang != null)
                                        {
                                            ReportHelperExcel.SetAlignment(rowC, dem++, "X", hStyleConCenter);
                                        }
                                        else
                                        {
                                            // Check có phải là thứ 7, cn khong
                                            string ngayCanCheck = ngay + "/" + thang + "/" + nam;
                                            DateTime ngayCK = DateTime.ParseExact(ngayCanCheck, "d/M/yyyy", CultureInfo.InvariantCulture);


                                            if (ngayCK.DayOfWeek == DayOfWeek.Saturday || ngayCK.DayOfWeek == DayOfWeek.Sunday)
                                            {
                                                ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConLeftBack);
                                            }
                                            else
                                            {
                                                ReportHelperExcel.SetAlignment(rowC, dem++, "0", hStyleConCenter);
                                            }
                                        }
                                    }
                                    // End for thu trong tuan cho tung nhan vien


                                }
                                var getLoaiNghi = nhanSuContext.sp_NS_BangTongHopCongThang(thang, nam, "", "", 0, "").Where(d => d.maNhanVien == item.maNhanVien).FirstOrDefault();
                                if (getLoaiNghi != null)
                                {
                                    ReportHelperExcel.SetAlignment(rowC, dem++, getLoaiNghi.congChuan, hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, getLoaiNghi.ngayQuet, hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, getLoaiNghi.congTac, hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, getLoaiNghi.nghiPhep, hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, getLoaiNghi.nghiBu, hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, getLoaiNghi.nghiKhacHuongLuong, hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, getLoaiNghi.nghiPhepKhongLuong, hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, getLoaiNghi.nghiLe, hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, getLoaiNghi.tangCaThuong, hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, getLoaiNghi.tangCaChuNhat, hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, getLoaiNghi.luyKeThangTruoc, hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, getLoaiNghi.tongCong, hStyleConRight);

                                }
                                else
                                {
                                    ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConRight);
                                    ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConRight);


                                }
                            }
                            if (demNVPhongBan == grpNhanVien)
                            {
                                idRowStart = idRowStart + 1;
                                var headerRowNCC = sheet.CreateRow(idRowStart);
                                string cellTitleNgChamCong = Convert.ToString(nguoiChamCong);
                                var titleCellNgChamCong = HSSFCellUtil.CreateCell(headerRowNCC, 1, cellTitleNgChamCong);
                                titleCellNgChamCong.CellStyle = styleTitle13;
                                string cellPhuTrachDonVi = Convert.ToString(phuTrachDonVi);
                                var titleCellPhuTrachDonVi = HSSFCellUtil.CreateCell(headerRowNCC, 10, cellPhuTrachDonVi);
                                titleCellPhuTrachDonVi.CellStyle = styleTitle13;
                                string cellngayThangNam = Convert.ToString(ngayThangNam);
                                var titleCellngayThangNam = HSSFCellUtil.CreateCell(headerRowNCC, 22, cellngayThangNam);
                                titleCellngayThangNam.CellStyle = styleTitle2;
                                idRowStart++;
                                var headerRowNDC = sheet.CreateRow(idRowStart);
                                string cellnguoiDuyetCong = Convert.ToString(nguoiDuyetCong);
                                var titleCellnguoiDuyetCong = HSSFCellUtil.CreateCell(headerRowNDC, 23, cellnguoiDuyetCong);
                                titleCellnguoiDuyetCong.CellStyle = styleTitle13;
                                idRowStart = idRowStart + 2;
                            }
                        }
                    }
                }
                idRowStart = idRowStart + 1;
                var headerRowNCCfooter = sheet.CreateRow(idRowStart);
                string cellTitleNgChamCongfooter = Convert.ToString("Ghi chú: X: Đi làm, 0: không đi làm. Bôi đen: thứ 7,CN");
                var titleCellNgChamCongfooter = HSSFCellUtil.CreateCell(headerRowNCCfooter, 1, cellTitleNgChamCongfooter);

                idRowStart = idRowStart + 2;




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
        #endregion
        #region Xuat File XuatFileBangTongHopCongThang
        public void XuatFileBangTongHopCongThang(string qSearch, string maPhongBan, int thang, int nam, int _page = 0)
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

                var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplateTHCT.xls"), FileMode.Open, FileAccess.Read);

                var workbook = new HSSFWorkbook(fileStream, true);

                filename += "BangTongHopCongThang_" + thang + "_" + nam + ".xls";

                #region format style excel cell
                /*style title start*/
                //tạo font cho các title
                //font tiêu đề 
                HSSFFont hFontTieuDe = (HSSFFont)workbook.CreateFont();
                hFontTieuDe.FontHeightInPoints = 11;
                hFontTieuDe.Boldweight = 100 * 10;
                hFontTieuDe.FontName = "Times New Roman";
                //hFontTieuDe.Color = HSSFColor.BLUE.index;

                //font tiêu đề 
                HSSFFont hFontTieuDeUnderline = (HSSFFont)workbook.CreateFont();
                hFontTieuDeUnderline.FontHeightInPoints = 11;
                hFontTieuDeUnderline.Boldweight = 100 * 10;
                hFontTieuDeUnderline.FontName = "Times New Roman";
                hFontTieuDeUnderline.Underline = 1;
                //hFontTieuDe.Color = HSSFColor.BLUE.index;


                HSSFFont hFontTieuDeItalic = (HSSFFont)workbook.CreateFont();
                hFontTieuDeItalic.FontHeightInPoints = 11;
                //hFontTieuDeItalic.Boldweight = 100 * 10;
                hFontTieuDeItalic.FontName = "Times New Roman";
                hFontTieuDeItalic.IsItalic = true;
                //hFontTieuDe.Color = HSSFColor.BLUE.index;


                HSSFFont hFontTieuDeLarge = (HSSFFont)workbook.CreateFont();
                hFontTieuDeLarge.FontHeightInPoints = 16;
                hFontTieuDeLarge.Boldweight = 100 * 10;
                hFontTieuDeLarge.FontName = "Times New Roman";
                //hFontTieuDeLarge.Color = HSSFColor.BLUE.index;

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

                //Set styleUnderline
                var styleTitleUnderline = workbook.CreateCellStyle();
                styleTitleUnderline.SetFont(hFontTieuDeUnderline);
                styleTitleUnderline.Alignment = HorizontalAlignment.LEFT;

                //Set style In nghiêng
                var styleTitleItalic = workbook.CreateCellStyle();
                styleTitleItalic.SetFont(hFontTieuDeItalic);
                styleTitleItalic.Alignment = HorizontalAlignment.LEFT;

                //Set style Large font
                var styleTitleLarge = workbook.CreateCellStyle();
                styleTitleLarge.SetFont(hFontTieuDeLarge);
                styleTitleLarge.Alignment = HorizontalAlignment.LEFT;

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

                //Khai báo row
                Row rowC = null;



                var sheet = workbook.CreateSheet("BangTongHopCongThang");

                //Khai báo row đầu tiên
                int firstRowNumber = 3;

                string cellTenCty = "Công ty TV Window";
                var titleCellCty = HSSFCellUtil.CreateCell(sheet.CreateRow(0), 0, cellTenCty.ToUpper());
                titleCellCty.CellStyle = styleTitle;

                string cellTenCacBanDH = "CÁC BAN ĐIỀU HÀNH DỰ ÁN";
                var titleCellTenCacBanDH = HSSFCellUtil.CreateCell(sheet.CreateRow(1), 1, cellTenCacBanDH.ToUpper());
                titleCellTenCacBanDH.CellStyle = styleTitle;

                string cellTitleMain = "BẢNG TỔNG HỢP CÔNG THÁNG " + thang + "/" + nam;
                var titleCellTitleMain = HSSFCellUtil.CreateCell(sheet.CreateRow(2), 5, cellTitleMain.ToUpper());
                titleCellTitleMain.CellStyle = styleTitle;

                firstRowNumber++;

                var list1 = new List<string>();
                list1.Add("STT");
                list1.Add("Họ tên");
                list1.Add("Mã nhân viên");
                list1.Add("Mã chấm công");
                list1.Add("Phòng ban");
                list1.Add("Chức vụ");
                list1.Add("Công chuẩn");
                list1.Add("Số ngày quét");
                list1.Add("Công tác");
                list1.Add("Nghỉ phép năm");
                list1.Add("Nghỉ bù");
                list1.Add("Nghỉ khác hưởng lương");
                list1.Add("Nghỉ phép không lương");
                list1.Add("Nghỉ lễ");
                list1.Add("Tăng ca thường");
                list1.Add("Tăng ca Chủ nhật");
                list1.Add("Tăng ca nghỉ lễ");
                list1.Add("Lũy kế tháng trước");
                list1.Add("Tổng cộng");

                var idRowStart = firstRowNumber; // bat dau o dong thu 4
                var headerRow = sheet.CreateRow(idRowStart);
                int rowend = idRowStart;
                ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);
                idRowStart++;
                sheet.SetColumnWidth(0, 5 * 210);
                sheet.SetColumnWidth(1, 30 * 210);
                sheet.SetColumnWidth(2, 15 * 210);
                sheet.SetColumnWidth(3, 15 * 210);
                sheet.SetColumnWidth(4, 30 * 210);
                sheet.SetColumnWidth(5, 15 * 210);
                sheet.SetColumnWidth(6, 15 * 210);
                sheet.SetColumnWidth(7, 15 * 210);
                sheet.SetColumnWidth(8, 15 * 210);
                sheet.SetColumnWidth(9, 15 * 210);
                sheet.SetColumnWidth(10, 15 * 210);
                sheet.SetColumnWidth(11, 15 * 210);
                sheet.SetColumnWidth(12, 15 * 400);
                sheet.SetColumnWidth(13, 15 * 300);
                sheet.SetColumnWidth(14, 15 * 300);
                sheet.SetColumnWidth(15, 15 * 300);
                sheet.SetColumnWidth(16, 15 * 300);
                sheet.SetColumnWidth(17, 15 * 300);


                var data = nhanSuContext.sp_NS_BangTongHopCongThang(thang, nam, qSearch, "", 0, maPhongBan).ToList();
                var stt = 0;
                int dem = 0;

                foreach (var item1 in data)
                {
                    dem = 0;

                    stt++;
                    idRowStart++;

                    rowC = sheet.CreateRow(idRowStart);
                    ReportHelperExcel.SetAlignment(rowC, dem++, stt.ToString(), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.hoTen, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.maNhanVien, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.maChamCong, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.phongBan, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.chucVu, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.congChuan, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.ngayQuet, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.congTac, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.nghiPhep, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.nghiBu, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.nghiKhacHuongLuong, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.nghiPhepKhongLuong, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.nghiLe, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.tangCaThuong, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.tangCaChuNhat, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.tangCaLe, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.luyKeThangTruoc, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.tongCong, hStyleConRight);
                }


                idRowStart = idRowStart + 2;
                var date = DateTime.Now.Day;
                string cellFooterNgayLap = "Tp.Hồ Chí Minh, ngày " + date + " tháng " + thang + " năm " + nam;
                var titleCellFooterNgayLap = HSSFCellUtil.CreateCell(sheet.CreateRow(idRowStart), 8, cellFooterNgayLap);
                titleCellFooterNgayLap.CellStyle = styleTitleItalic;

                //idRowStart = idRowStart + 2;
                //string cellFooterPTC = "PHÒNG TỔ CHỨC CB-LĐ";
                //var titleCellFooterPTC = HSSFCellUtil.CreateCell(sheet.CreateRow(idRowStart), 1, cellFooterPTC);
                //titleCellFooterPTC.CellStyle = styleTitle;

                //string cellFooterKT = "PHÒNG TÀI CHÍNH KẾ TOÁN";
                //var titleCellFooterKT = HSSFCellUtil.CreateCell(sheet.GetRow(idRowStart), 7, cellFooterKT);
                //titleCellFooterKT.CellStyle = styleTitle;

                //string cellFooterTGD = "TỔNG GIÁM ĐỐC";
                //var titleCellFooterTGD = HSSFCellUtil.CreateCell(sheet.GetRow(idRowStart), 14, cellFooterTGD);
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
        public void XuatFileChiTietGioQuet(int thang, int nam, string qSearch)
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

                var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplateTHCT.xls"), FileMode.Open, FileAccess.Read);

                var workbook = new HSSFWorkbook(fileStream, true);

                filename += "BangChamCongChiTietThang_" + thang + "_" + nam + ".xls";

                #region format style excel cell
                /*style title start*/
                //tạo font cho các title
                //font tiêu đề 
                HSSFFont hFontTieuDe = (HSSFFont)workbook.CreateFont();
                hFontTieuDe.FontHeightInPoints = 11;
                hFontTieuDe.Boldweight = 100 * 10;
                hFontTieuDe.FontName = "Times New Roman";
                //hFontTieuDe.Color = HSSFColor.BLUE.index;

                //font tiêu đề 
                HSSFFont hFontTieuDeUnderline = (HSSFFont)workbook.CreateFont();
                hFontTieuDeUnderline.FontHeightInPoints = 11;
                hFontTieuDeUnderline.Boldweight = 100 * 10;
                hFontTieuDeUnderline.FontName = "Times New Roman";
                hFontTieuDeUnderline.Underline = 1;
                //hFontTieuDe.Color = HSSFColor.BLUE.index;


                HSSFFont hFontTieuDeItalic = (HSSFFont)workbook.CreateFont();
                hFontTieuDeItalic.FontHeightInPoints = 11;
                //hFontTieuDeItalic.Boldweight = 100 * 10;
                hFontTieuDeItalic.FontName = "Times New Roman";
                hFontTieuDeItalic.IsItalic = true;
                //hFontTieuDe.Color = HSSFColor.BLUE.index;


                HSSFFont hFontTieuDeLarge = (HSSFFont)workbook.CreateFont();
                hFontTieuDeLarge.FontHeightInPoints = 16;
                hFontTieuDeLarge.Boldweight = 100 * 10;
                hFontTieuDeLarge.FontName = "Times New Roman";
                //hFontTieuDeLarge.Color = HSSFColor.BLUE.index;

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

                //Set styleUnderline
                var styleTitleUnderline = workbook.CreateCellStyle();
                styleTitleUnderline.SetFont(hFontTieuDeUnderline);
                styleTitleUnderline.Alignment = HorizontalAlignment.LEFT;

                //Set style In nghiêng
                var styleTitleItalic = workbook.CreateCellStyle();
                styleTitleItalic.SetFont(hFontTieuDeItalic);
                styleTitleItalic.Alignment = HorizontalAlignment.LEFT;

                //Set style Large font
                var styleTitleLarge = workbook.CreateCellStyle();
                styleTitleLarge.SetFont(hFontTieuDeLarge);
                styleTitleLarge.Alignment = HorizontalAlignment.LEFT;

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

                //Khai báo row
                Row rowC = null;



                var sheet = workbook.CreateSheet("BangChiTietGioQuet");

                //Khai báo row đầu tiên
                int firstRowNumber = 3;

                string cellTenCty = "Công ty TV Window";
                var titleCellCty = HSSFCellUtil.CreateCell(sheet.CreateRow(0), 0, cellTenCty.ToUpper());
                titleCellCty.CellStyle = styleTitle;

                string cellTenCacBanDH = "CÁC BAN ĐIỀU HÀNH DỰ ÁN";
                var titleCellTenCacBanDH = HSSFCellUtil.CreateCell(sheet.CreateRow(1), 1, cellTenCacBanDH.ToUpper());
                titleCellTenCacBanDH.CellStyle = styleTitle;

                string cellTitleMain = "BẢNG CHẤM CÔNG CHI TIẾT THÁNG " + thang + "/" + nam;
                var titleCellTitleMain = HSSFCellUtil.CreateCell(sheet.CreateRow(2), 3, cellTitleMain.ToUpper());
                titleCellTitleMain.CellStyle = styleTitle;

                firstRowNumber++;

                var list1 = new List<string>();
                list1.Add("STT");
                list1.Add("Họ tên");
                list1.Add("Mã nhân viên");
                list1.Add("Mã vân tay");
                list1.Add("Ngày quét");
                list1.Add("Tên ngày");
                list1.Add("Giờ vào");
                list1.Add("Giờ ra");
                list1.Add("Loại");
                list1.Add("Số giờ công");
                var idRowStart = firstRowNumber; // bat dau o dong thu 4
                var headerRow = sheet.CreateRow(idRowStart);
                int rowend = idRowStart;
                ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);
                idRowStart++;
                sheet.SetColumnWidth(0, 5 * 210);
                sheet.SetColumnWidth(1, 30 * 210);
                sheet.SetColumnWidth(2, 15 * 210);
                sheet.SetColumnWidth(3, 15 * 210);
                sheet.SetColumnWidth(4, 30 * 210);
                sheet.SetColumnWidth(5, 15 * 210);
                sheet.SetColumnWidth(6, 15 * 210);
                sheet.SetColumnWidth(7, 15 * 210);
                sheet.SetColumnWidth(8, 15 * 210);
                sheet.SetColumnWidth(9, 15 * 210);
                sheet.SetColumnWidth(10, 15 * 210);


                var data = nhanSuContext.sp_NS_BangChamCongChiTiet(thang, nam, qSearch, "").ToList();
                var stt = 0;
                int dem = 0;

                foreach (var item1 in data)
                {
                    dem = 0;

                    stt++;
                    idRowStart++;

                    rowC = sheet.CreateRow(idRowStart);
                    ReportHelperExcel.SetAlignment(rowC, dem++, stt.ToString(), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.hoTen, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.maNhanVien, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.maVanTay, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0: dd/MM/yyyy}", item1.ngayLam), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.ngayLam.Value.DayOfWeek.ToString(), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.quetVao.Value.ToString("HH:mm:ss"), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.quetRa.Value.ToString("HH:mm:ss"), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.loai, hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.soGioCong, hStyleConCenter);
                }


                idRowStart = idRowStart + 2;
                var date = DateTime.Now.Day;
                string cellFooterNgayLap = "Tp.Hồ Chí Minh, ngày " + date + " tháng " + thang + " năm " + nam;
                var titleCellFooterNgayLap = HSSFCellUtil.CreateCell(sheet.CreateRow(idRowStart), 8, cellFooterNgayLap);
                titleCellFooterNgayLap.CellStyle = styleTitleItalic;

                //idRowStart = idRowStart + 2;
                //string cellFooterPTC = "PHÒNG TỔ CHỨC CB-LĐ";
                //var titleCellFooterPTC = HSSFCellUtil.CreateCell(sheet.CreateRow(idRowStart), 1, cellFooterPTC);
                //titleCellFooterPTC.CellStyle = styleTitle;

                //string cellFooterKT = "PHÒNG TÀI CHÍNH KẾ TOÁN";
                //var titleCellFooterKT = HSSFCellUtil.CreateCell(sheet.GetRow(idRowStart), 7, cellFooterKT);
                //titleCellFooterKT.CellStyle = styleTitle;

                //string cellFooterTGD = "TỔNG GIÁM ĐỐC";
                //var titleCellFooterTGD = HSSFCellUtil.CreateCell(sheet.GetRow(idRowStart), 14, cellFooterTGD);
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
        #endregion
        public ActionResult XemBangChamCongTongHop()
        {

            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion


            thang(DateTime.Now.Month);
            nam(DateTime.Now.Year);
            return View("");
        }
        public ActionResult LoadBangChamCongTongHop(string qSearch, string maPhongBan, int thang, int nam, int _page = 0)
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
            // int total = nhanSuContext.sp_NS_BangTongHopCongThang(thang, nam, qSearch, "", 0, maPhongBan).Count();
            //  PagingLoaderController("/BangChamCongTongHop/LoadBangChamCongTongHop/", total, page, "?qsearch=" + qSearch);
            //ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangTongHopCongThang(thang, nam, qSearch, "", 0, maPhongBan).Skip(start).Take(offset).ToList();
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangTongHopCongThang(thang, nam, qSearch, "", 0, maPhongBan).ToList();
            ViewData["qSearch"] = qSearch;
            Administrator(GetUser().manv);
            return PartialView("_LoadBangChamCongTongHop");
        }
        public ActionResult UpdateBangChamCongTH(int thang, int nam)
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

                var list = nhanSuContext.sp_NS_BangTongHopCongThang(thang, nam, null, null, 1, null);
                var result = new { kq = true };
                SaveActiveHistory("Tính công tháng: " + thang + " năm: " + nam);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return View();
            }

        }
        public ActionResult UpdateBangChamCongChiTiet(int thang, int nam)
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
                SaveActiveHistory("Tính công chi tiết: " + thang + " năm: " + nam);
                var list = nhanSuContext.sp_Ns_CapNhatBangCongChiTiet(thang, nam);


                var result = new { kq = true };

                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return View();
            }

        }
        public ActionResult ImportDuLieuVanTay(int thang, int nam, string maCongTrinh, int TuNgay, int DenNgay)
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
                maCongTrinh = "VP";
                DBChamCongDataContext linqChamCong = new DBChamCongDataContext();
                var lstMayChamCong = linqChamCong.Sp_ViewMachinePB(maCongTrinh, null).Distinct().Where(d => d.trangThai == 1).Select(d => d.machineId).ToList();
                LinqNhanSuDataContext linqNhanSu = new LinqNhanSuDataContext();

                var lstMaVanTay_NhanSu = linqNhanSu.GetTable<tbl_NS_ChamCong>().Where(d => d.checkTime.Value.Year == nam && d.checkTime.Value.Month == thang && d.checkTime.Value.Day >= TuNgay && d.checkTime.Value.Day <= DenNgay && lstMayChamCong.Contains(d.maMayChamCong)).Select(d => d.maChamCong).Distinct().ToList();
                //int?[] lstVanTay = linqNhanSu.tbl_NS_ChamCongs.Where(d => d.checkTime.Value.Year == nam && d.checkTime.Value.Month == thang).Select(d => d.idQuet).ToArray();

                var lstVanTay_MayChamCong = (from p in linqChamCong.userinfos
                                             join q in linqChamCong.checkinouts on p.userid equals q.userid
                                             where (q.checktime.Value.Month == thang && q.checktime.Value.Year == nam && q.checktime.Value.Day >= TuNgay && q.checktime.Value.Day <= DenNgay && lstMayChamCong.Contains(q.SN) && lstMaVanTay_NhanSu.Contains(p.badgenumber))
                                             select new
                                             {
                                                 userid = p.userid,
                                                 maChamCong = p.badgenumber,
                                                 maMayChamCong = q.SN,
                                                 idQuet = q.id,
                                                 checktime = q.checktime


                                             }).ToList();

                int soLuotImport = 0;
                foreach (var item in lstVanTay_MayChamCong)
                {
                    var checkTblChamCong_NhanSu = linqNhanSu.tbl_NS_ChamCongs.Where(d => d.idQuet == item.idQuet).FirstOrDefault();
                    if (checkTblChamCong_NhanSu == null)
                    {
                        tbl_NS_ChamCong tblChamCong = new tbl_NS_ChamCong();
                        tblChamCong.checkTime = item.checktime;
                        tblChamCong.maMayChamCong = item.maMayChamCong;
                        tblChamCong.idQuet = item.idQuet;
                        tblChamCong.maChamCong = item.maChamCong;
                        linqNhanSu.tbl_NS_ChamCongs.InsertOnSubmit(tblChamCong);
                        linqNhanSu.SubmitChanges();
                        soLuotImport++;
                    }
                }

                SaveActiveHistory("Import dữ liệu vân tay: " + thang + " năm: " + nam + ". Import được: " + soLuotImport);
                var result = new { kq = true, soLuotImport = soLuotImport };

                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                SaveActiveHistory("Import dữ liệu vân tay: " + thang + " năm: " + nam + " thật bại.");
                var result = new { kq = false };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult DuyetTongHopCongNV(int thang, int nam)
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
                var checkEx = nhanSuContext.tbl_DuyetBangTongHopCongNVs.Where(t => t.nam == nam && t.thang == thang).FirstOrDefault();
                if (checkEx != null)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                //End check
                // Insert Row 
                tbl_DuyetBangTongHopCongNV tblDuyetBL = new tbl_DuyetBangTongHopCongNV();
                tblDuyetBL.nam = nam;
                tblDuyetBL.thang = thang;
                tblDuyetBL.ngayDuyet = DateTime.Now;
                tblDuyetBL.nguoiDuyet = GetUser().manv;
                nhanSuContext.tbl_DuyetBangTongHopCongNVs.InsertOnSubmit(tblDuyetBL);
                nhanSuContext.SubmitChanges();
                // End Insert Row
                // Check Exist
                var list = nhanSuContext.tbl_DuyetBangTongHopCongNVs.Where(t => t.nam == nam && t.thang == thang).FirstOrDefault();

                if (list != null)
                {
                    result = new { kq = true };
                }
                SaveActiveHistory("Duyệt tổng hợp công tháng: " + thang + " năm: " + nam);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return View();
            }

        }
        public ActionResult CheckDuyetTongHopCong(int thang, int nam)
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

                var checkEx = nhanSuContext.tbl_DuyetBangTongHopCongNVs.Where(t => t.nam == nam && t.thang == thang).FirstOrDefault();
                if (checkEx != null)
                {
                    var result = new { kq = true };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    var result = new { kq = false };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                //End check
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
            ViewData["thang"] = new SelectList(dics, "Key", "Value", value);
            ViewData["thangtc"] = new SelectList(dics, "Key", "Value", value);
            ViewData["thangImport"] = new SelectList(dics, "Key", "Value", value);
        }
        private void nam(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 2015; i < 2031; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["nam"] = new SelectList(dics, "Key", "Value", value);
            ViewData["namtc"] = new SelectList(dics, "Key", "Value", value);
            ViewData["namImport"] = new SelectList(dics, "Key", "Value", value);
        }
        private void TuNgay(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 1; i <= 31; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["TuNgay"] = new SelectList(dics, "Key", "Value", value);
        }
        private void DenNgay(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 1; i <= 31; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["DenNgay"] = new SelectList(dics, "Key", "Value", value);
        }

        #region Import dữ liệu vân tay
        public void DoDuLieuVanTay()
        {
            int TuNgay = 01;
            int DenNgay = 30;
            int thang = 4;
            int nam = 2022;

            DBChamCongDataContext linqChamCong = new DBChamCongDataContext();
            LinqNhanSuDataContext linqNhanSu = new LinqNhanSuDataContext();
            var lstVanTay_MayChamCong = linqChamCong.sp_LayChamCongConThieuVeERP(string.Empty, "6269154200022", string.Empty, TuNgay, DenNgay, thang, nam).ToList();

            int soLuotImport = 0;

            foreach (var item in lstVanTay_MayChamCong)
            {
                var checkTblChamCong_NhanSu = linqNhanSu.tbl_NS_ChamCongs.Where(d => d.idQuet == item.idQuet && d.checkTime.Value.Year == nam && d.checkTime.Value.Month == thang).FirstOrDefault();

                if (checkTblChamCong_NhanSu == null)
                {
                    tbl_NS_ChamCong tblChamCong = new tbl_NS_ChamCong();

                    tblChamCong.checkTime = item.checktime;
                    //Convert.ToDateTime("2019-09-18 08:25:02.000");
                    //Convert.ToDateTime("2019-09-18 08:25:02.000");                        
                    //Convert.ToDateTime("2019-09-18 08:25:02.000");                       
                    tblChamCong.maMayChamCong = item.SN;
                    tblChamCong.idQuet = item.idQuet;
                    tblChamCong.maChamCong = item.badgenumber;
                    linqNhanSu.tbl_NS_ChamCongs.InsertOnSubmit(tblChamCong);
                    linqNhanSu.SubmitChanges();
                    soLuotImport++;
                }
            }

            SaveActiveHistory("Import dữ liệu vân tay: " + thang + " năm: " + nam + ". Import được: " + soLuotImport);
        }
        #endregion

        #region Điều chỉnh công
        public JsonResult DieuChinhCong(string maNhanVienDieuChinh, int thangDieuChinh, int namDieuChinh, float? congDieuChinh)
        {
            try
            {
                var capNhatCongDC = nhanSuContext.tbl_NS_BangTongHopCongThangs.Where(d => d.maNhanVien == maNhanVienDieuChinh && d.thang == thangDieuChinh && d.nam == namDieuChinh).FirstOrDefault();
                capNhatCongDC.tongCong = Math.Round(congDieuChinh ?? 0, 2, MidpointRounding.ToEven);
                nhanSuContext.SubmitChanges();
                return Json(string.Empty);
            }
            catch
            {
                return Json("Error");
            }
        }
        #endregion

        #region Import file tổng hợp công

        public void XuatFileBangTongHopCongThangMau(string qSearch, string maPhongBan, int thang, int nam, int _page = 0)
        {
            try
            {
                var filename = "";
                var virtualPath = HttpRuntime.AppDomainAppVirtualPath;

                var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplateTHCT.xls"), FileMode.Open, FileAccess.Read);

                var workbook = new HSSFWorkbook(fileStream, true);

                filename += "BangTongHopCongThang_" + thang + "_" + nam + ".xls";

                #region format style excel cell
                /*style title start*/
                //tạo font cho các title
                //font tiêu đề 
                HSSFFont hFontTieuDe = (HSSFFont)workbook.CreateFont();
                hFontTieuDe.FontHeightInPoints = 11;
                hFontTieuDe.Boldweight = 100 * 10;
                hFontTieuDe.FontName = "Times New Roman";
                //hFontTieuDe.Color = HSSFColor.BLUE.index;

                //font tiêu đề 
                HSSFFont hFontTieuDeUnderline = (HSSFFont)workbook.CreateFont();
                hFontTieuDeUnderline.FontHeightInPoints = 11;
                hFontTieuDeUnderline.Boldweight = 100 * 10;
                hFontTieuDeUnderline.FontName = "Times New Roman";
                hFontTieuDeUnderline.Underline = 1;
                //hFontTieuDe.Color = HSSFColor.BLUE.index;


                HSSFFont hFontTieuDeItalic = (HSSFFont)workbook.CreateFont();
                hFontTieuDeItalic.FontHeightInPoints = 11;
                //hFontTieuDeItalic.Boldweight = 100 * 10;
                hFontTieuDeItalic.FontName = "Times New Roman";
                hFontTieuDeItalic.IsItalic = true;
                //hFontTieuDe.Color = HSSFColor.BLUE.index;


                HSSFFont hFontTieuDeLarge = (HSSFFont)workbook.CreateFont();
                hFontTieuDeLarge.FontHeightInPoints = 16;
                hFontTieuDeLarge.Boldweight = 100 * 10;
                hFontTieuDeLarge.FontName = "Times New Roman";
                //hFontTieuDeLarge.Color = HSSFColor.BLUE.index;

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

                //Set styleUnderline
                var styleTitleUnderline = workbook.CreateCellStyle();
                styleTitleUnderline.SetFont(hFontTieuDeUnderline);
                styleTitleUnderline.Alignment = HorizontalAlignment.LEFT;

                //Set style In nghiêng
                var styleTitleItalic = workbook.CreateCellStyle();
                styleTitleItalic.SetFont(hFontTieuDeItalic);
                styleTitleItalic.Alignment = HorizontalAlignment.LEFT;

                //Set style Large font
                var styleTitleLarge = workbook.CreateCellStyle();
                styleTitleLarge.SetFont(hFontTieuDeLarge);
                styleTitleLarge.Alignment = HorizontalAlignment.LEFT;

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

                //Khai báo row
                Row rowC = null;

                var sheet = workbook.CreateSheet("BangTongHopCongThang");

                //Khai báo row đầu tiên
                int firstRowNumber = 0;

                var list1 = new List<string>();
                list1.Add("STT");
                list1.Add("Họ tên");
                list1.Add("Mã nhân viên");
                list1.Add("Mã chấm công");
                list1.Add("Phòng ban");
                list1.Add("Chức vụ");
                list1.Add("Công chuẩn");
                list1.Add("Số ngày quét");
                list1.Add("Công tác");
                list1.Add("Nghỉ phép có lương");
                list1.Add("Nghỉ phép không lương");
                list1.Add("Nghỉ lễ");
                list1.Add("Lũy kế tháng trước");
                list1.Add("Tổng cộng");
                list1.Add("Tổng công điều chỉnh");
                //list1.Add("Kết quả đánh giá");
                list1.Add("Ghi chú");

                var idRowStart = firstRowNumber; // bat dau o dong thu 4
                var headerRow = sheet.CreateRow(idRowStart);
                int rowend = idRowStart;
                ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);
                idRowStart++;
                sheet.SetColumnWidth(0, 5 * 210);
                sheet.SetColumnWidth(1, 30 * 210);
                sheet.SetColumnWidth(2, 15 * 210);
                sheet.SetColumnWidth(3, 15 * 210);
                sheet.SetColumnWidth(4, 30 * 210);
                sheet.SetColumnWidth(5, 15 * 210);
                sheet.SetColumnWidth(6, 15 * 210);
                sheet.SetColumnWidth(7, 15 * 210);
                sheet.SetColumnWidth(8, 15 * 210);
                sheet.SetColumnWidth(9, 15 * 210);
                sheet.SetColumnWidth(10, 15 * 210);
                sheet.SetColumnWidth(11, 15 * 210);
                sheet.SetColumnWidth(12, 15 * 400);
                sheet.SetColumnWidth(13, 15 * 300);
                sheet.SetColumnWidth(14, 20 * 300);
                sheet.SetColumnWidth(15, 20 * 300);
                sheet.SetColumnWidth(16, 15 * 300);
                var data = nhanSuContext.sp_NS_BangTongHopCongThang(thang, nam, qSearch, "", 0, maPhongBan).ToList();
                var stt = 0;
                int dem = 0;

                foreach (var item1 in data)
                {
                    dem = 0;

                    stt++;
                    idRowStart++;

                    rowC = sheet.CreateRow(idRowStart);
                    ReportHelperExcel.SetAlignment(rowC, dem++, stt.ToString(), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.hoTen, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.maNhanVien, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.maChamCong, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.phongBan, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.chucVu, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.congChuan, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.ngayQuet, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.congTac, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, (item1.nghiPhep ?? 0) + (item1.nghiBu ?? 0), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.nghiPhepKhongLuong, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.nghiLe, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.luyKeThangTruoc, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.congDieuChinh ?? item1.tongCong, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.tongCong, hStyleConRight);
                    //ReportHelperExcel.SetAlignment(rowC, dem++, item1.ketQuaDanhGiaLamViec, hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, dem++, item1.ghiChu, hStyleConRight);
                }

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

        public ActionResult ImportFileTC(int? thang, int? nam)
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
                        string savedLocation = "/UploadFiles/NhanVien/";
                        Directory.CreateDirectory(savedLocation);
                        var filePath = Server.MapPath(savedLocation);
                        string fileName = date.ToString() + File.FileName;
                        string savedFileName = Path.Combine(filePath, fileName);
                        File.SaveAs(savedFileName);

                        ExcelDataProcessing excelDataProcessor = new ExcelDataProcessing(savedFileName);
                        DataTable dt = excelDataProcessor.GetDataTableWorkSheet("BangTongHopCongThang");
                        List<tbl_NS_BangTongHopCongThang> listTHC = new List<tbl_NS_BangTongHopCongThang>();
                        tbl_NS_BangTongHopCongThang congTH;


                        foreach (DataRow row in dt.Rows)
                        {

                            if (!String.IsNullOrEmpty(row["Mã nhân viên"].ToString()))
                            {
                                var tongHopCong = nhanSuContext.tbl_NS_BangTongHopCongThangs.Where(d => d.thang == thang && d.nam == nam && d.maNhanVien.Trim().ToLower() == row["Mã nhân viên"].ToString().Trim().ToLower()).FirstOrDefault();
                                if (tongHopCong != null)
                                {
                                    tongHopCong.congChuan = string.IsNullOrEmpty(row["Công chuẩn"].ToString()) ? 0 : Convert.ToDouble(row["Công chuẩn"].ToString());
                                    tongHopCong.ngayQuet = string.IsNullOrEmpty(row["Số ngày quét"].ToString()) ? 0 : Convert.ToDouble(row["Số ngày quét"].ToString());
                                    tongHopCong.congTac = string.IsNullOrEmpty(row["Công tác"].ToString()) ? 0 : Convert.ToDouble(row["Công tác"].ToString());
                                    tongHopCong.nghiPhep = string.IsNullOrEmpty(row["Nghỉ phép có lương"].ToString()) ? 0 : Convert.ToDouble(row["Nghỉ phép có lương"].ToString());
                                    tongHopCong.nghiPhepKhongLuong = string.IsNullOrEmpty(row["Nghỉ phép không lương"].ToString()) ? 0 : Convert.ToDouble(row["Nghỉ phép không lương"].ToString());
                                    tongHopCong.nghiLe = string.IsNullOrEmpty(row["Nghỉ lễ"].ToString()) ? 0 : Convert.ToDouble(row["Nghỉ lễ"].ToString());
                                    tongHopCong.luyKeThangTruoc = string.IsNullOrEmpty(row["Lũy kế tháng trước"].ToString()) ? 0 : Convert.ToDouble(row["Lũy kế tháng trước"].ToString());
                                    tongHopCong.tongCong = string.IsNullOrEmpty(row["Tổng công điều chỉnh"].ToString()) ? 0 : Math.Round(Convert.ToDouble(row["Tổng công điều chỉnh"].ToString()), 2, MidpointRounding.ToEven);
                                    tongHopCong.congDieuChinh = string.IsNullOrEmpty(row["Tổng cộng"].ToString()) ? 0 : Math.Round(Convert.ToDouble(row["Tổng cộng"].ToString()), 2, MidpointRounding.ToEven);
                                    //tongHopCong.ketQuaDanhGiaLamViec = string.IsNullOrEmpty(row["Kết quả đánh giá"].ToString()) ? 0 : Math.Round(Convert.ToDouble(row["Kết quả đánh giá"].ToString()), 2, MidpointRounding.ToEven);

                                    nhanSuContext.SubmitChanges();
                                }
                                else
                                {
                                    congTH = new tbl_NS_BangTongHopCongThang();
                                    congTH.thang = thang ?? DateTime.Now.Month;
                                    congTH.nam = nam ?? DateTime.Now.Year;
                                    congTH.congChuan = string.IsNullOrEmpty(row["Công chuẩn"].ToString()) ? 0 : Convert.ToDouble(row["Công chuẩn"].ToString());
                                    congTH.ngayQuet = string.IsNullOrEmpty(row["Số ngày quét"].ToString()) ? 0 : Convert.ToDouble(row["Số ngày quét"].ToString());
                                    congTH.congTac = string.IsNullOrEmpty(row["Công tác"].ToString()) ? 0 : Convert.ToDouble(row["Công tác"].ToString());
                                    congTH.nghiPhep = string.IsNullOrEmpty(row["Nghỉ phép có lương"].ToString()) ? 0 : Convert.ToDouble(row["Nghỉ phép có lương"].ToString());
                                    congTH.nghiPhepKhongLuong = string.IsNullOrEmpty(row["Nghỉ phép không lương"].ToString()) ? 0 : Convert.ToDouble(row["Nghỉ phép không lương"].ToString());
                                    congTH.nghiLe = string.IsNullOrEmpty(row["Nghỉ lễ"].ToString()) ? 0 : Convert.ToDouble(row["Nghỉ lễ"].ToString());
                                    congTH.luyKeThangTruoc = string.IsNullOrEmpty(row["Lũy kế tháng trước"].ToString()) ? 0 : Convert.ToDouble(row["Lũy kế tháng trước"].ToString());
                                    congTH.tongCong = string.IsNullOrEmpty(row["Tổng công điều chỉnh"].ToString()) ? 0 : Math.Round(Convert.ToDouble(row["Tổng công điều chỉnh"].ToString()), 2, MidpointRounding.ToEven);
                                    congTH.congDieuChinh = string.IsNullOrEmpty(row["Tổng cộng"].ToString()) ? 0 : Math.Round(Convert.ToDouble(row["Tổng cộng"].ToString()), 2, MidpointRounding.ToEven);
                                    // congTH.ketQuaDanhGiaLamViec = string.IsNullOrEmpty(row["Kết quả đánh giá"].ToString()) ? 0 : Math.Round(Convert.ToDouble(row["Kết quả đánh giá"].ToString()), 2, MidpointRounding.ToEven);
                                    congTH.ghiChu = row["Ghi chú"].ToString().Trim();
                                    congTH.hoTen = row["Họ tên"].ToString().Trim();
                                    congTH.maNhanVien = row["Mã nhân viên"].ToString().Trim();
                                    congTH.maChamCong = row["Mã chấm công"].ToString().Trim();
                                    congTH.phongBan = row["Phòng ban"].ToString().Trim();
                                    congTH.chucVu = row["Chức vụ"].ToString().Trim();


                                    listTHC.Add(congTH);
                                }


                            }
                        }

                        nhanSuContext.tbl_NS_BangTongHopCongThangs.InsertAllOnSubmit(listTHC);
                        nhanSuContext.SubmitChanges();

                        // System.IO.File.Delete(Server.MapPath("/UploadFiles/NhanVien/" + fileName));
                    }
                }
                SaveActiveHistory("Import file điều chỉnh công tổng cộng tháng " + thang + " năm " + nam);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
        }
        #endregion

    }
}
