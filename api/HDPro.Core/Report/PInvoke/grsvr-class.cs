//grsvr-class.cs: 由锐浪报表接口维护工具自动生成，请勿手动修改
//生成时间: 2023/03/09 11:26:41

using System;
using System.Runtime.InteropServices;

[module: DefaultCharSet(CharSet.Unicode)]

namespace gridreport
{
    public static class RawAPI
    {
        // private const string LibFileName = "grsvrl6.dll"; //Windows下用这个参数
        private const string LibFileName = "libgrsvrl6.so"; //Linux下用这个参数

        public delegate void GeneralEventFun(IntPtr reportHandle);
        public delegate void IntArgEventFun(IntPtr reportHandle, int val);
        public delegate void StringArgEventFun(IntPtr reportHandle, string str);
        public delegate void RuntimeErrorEventFun(IntPtr reportHandle, int errorID, string errorMsg);
        public delegate void GroupEventFun(IntPtr reportHandle, IntPtr groupHandle);
        public delegate void SectionEventFun(IntPtr reportHandle, IntPtr sectionHandle);
        public delegate void FieldEventFun(IntPtr reportHandle, IntPtr fieldHandle);
        public delegate void TextBoxEventFun(IntPtr reportHandle, IntPtr textBoxHandle);
        public delegate void ControlCustomDrawEventFun(IntPtr reportHandle, IntPtr controlHandle, IntPtr graphicsHandle);
        public delegate void ChartEventFun(IntPtr reportHandle, IntPtr chartHandle);
        public delegate void ExportEventFun(IntPtr reportHandle, IntPtr exportOptionHandle);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventInitialize2(IntPtr handle, GeneralEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventFetchRecord2(IntPtr handle, GeneralEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventBeforePostRecord2(IntPtr handle, GeneralEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventBeforeSort2(IntPtr handle, StringArgEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventRuntimeError2(IntPtr handle, RuntimeErrorEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventProcessBegin2(IntPtr handle, GeneralEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventProcessEnd2(IntPtr handle, GeneralEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventGroupBegin2(IntPtr handle, GroupEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventGroupEnd2(IntPtr handle, GroupEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventProcessRecord2(IntPtr handle, GeneralEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventGeneratePagesBegin2(IntPtr handle, GeneralEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventGeneratePagesEnd2(IntPtr handle, GeneralEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventPageProcessRecord2(IntPtr handle, GeneralEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventPageStart2(IntPtr handle, GeneralEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventPageEnd2(IntPtr handle, GeneralEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventSectionFormat2(IntPtr handle, SectionEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventFieldGetDisplayText2(IntPtr handle, FieldEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventTextBoxGetDisplayText2(IntPtr handle, TextBoxEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventControlCustomDraw2(IntPtr handle, ControlCustomDrawEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventChartRequestData2(IntPtr handle, ChartEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventPrintBegin2(IntPtr handle, GeneralEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventPrintEnd2(IntPtr handle, GeneralEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventPrintAborted2(IntPtr handle, GeneralEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventPrintPage2(IntPtr NativeHandle, IntArgEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventExportBegin2(IntPtr handle, ExportEventFun cb);

        [DllImport(LibFileName)]
        public static extern void GRAttachEventExportEnd2(IntPtr handle, ExportEventFun cb);


        [DllImport(LibFileName)]
        public static extern void GRInitConfig(string ModulePath);

        [DllImport(LibFileName)]
        public static extern void GRSetConsoleMsgAsUTF8(bool Val);

        [DllImport(LibFileName)]
        public static extern bool GRIsModuleInitialized();

        [DllImport(LibFileName)]
        public static extern IntPtr GRGetModulePath();

        [DllImport(LibFileName)]
        public static extern IntPtr GRGetModuleInfo();

        [DllImport(LibFileName)]
        public static extern IntPtr GRGetModuleVersion();

        [DllImport(LibFileName)]
        public static extern IntPtr GRGenerateBarcodeGraph(string BarcodeParams);

        [DllImport(LibFileName)]
        public static extern IntPtr GRReportCreate();

        [DllImport(LibFileName)]
        public static extern void GRReportRelease(IntPtr Handle);


        //Font原始API
        [DllImport(LibFileName)]
        public static extern bool GRFontGetBold(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFontSetBold(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern int GRFontGetCharset(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFontSetCharset(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern bool GRFontGetItalic(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFontSetItalic(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFontGetName(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFontSetName(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRFontGetParentFont(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern double GRFontGetPoint(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFontSetPoint(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern bool GRFontGetStrikethrough(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFontSetStrikethrough(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRFontGetUnderline(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFontSetUnderline(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern int GRFontGetWeight(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFontSetWeight(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFontClone(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRFontFree(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRFontToDefault(IntPtr Handle);

        //Pen原始API
        [DllImport(LibFileName)]
        public static extern uint GRPenGetColor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPenSetColor(IntPtr handle, uint val);
        [DllImport(LibFileName)]
        public static extern PenStyle GRPenGetStyle(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPenSetStyle(IntPtr handle, PenStyle val);
        [DllImport(LibFileName)]
        public static extern double GRPenGetWidth(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPenSetWidth(IntPtr handle, double val);

        //Border原始API
        [DllImport(LibFileName)]
        public static extern int GRBorderGetInnerIndent(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBorderSetInnerIndent(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRBorderGetInnerPen(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRBorderGetInnerStyles(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBorderSetInnerStyles(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRBorderGetPen(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRBorderGetShadow(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBorderSetShadow(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern uint GRBorderGetShadowColor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBorderSetShadowColor(IntPtr handle, uint val);
        [DllImport(LibFileName)]
        public static extern int GRBorderGetShadowWidth(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBorderSetShadowWidth(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern int GRBorderGetStyles(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBorderSetStyles(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern bool GRBorderGetInnerStyle(IntPtr Handle, BorderStyles BorderSide);
        [DllImport(LibFileName)]
        public static extern bool GRBorderGetStyle(IntPtr Handle, BorderStyles BorderSide);
        [DllImport(LibFileName)]
        public static extern void GRBorderSetInnerStyle(IntPtr Handle, BorderStyles BorderSide, bool ToShow);
        [DllImport(LibFileName)]
        public static extern void GRBorderSetStyle(IntPtr Handle, BorderStyles BorderSide, bool ToShow);

        //TextFormat原始API
        [DllImport(LibFileName)]
        public static extern double GRTextFormatGetCharacterSpacing(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRTextFormatSetCharacterSpacing(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern bool GRTextFormatGetEndEllipsis(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRTextFormatSetEndEllipsis(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern double GRTextFormatGetFirstCharIndent(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRTextFormatSetFirstCharIndent(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRTextFormatGetFontWidthRatio(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRTextFormatSetFontWidthRatio(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern bool GRTextFormatGetHtmlTags(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRTextFormatSetHtmlTags(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern double GRTextFormatGetLineSpacing(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRTextFormatSetLineSpacing(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRTextFormatGetParagraphSpacing(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRTextFormatSetParagraphSpacing(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern TextAlign GRTextFormatGetTextAlign(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRTextFormatSetTextAlign(IntPtr handle, TextAlign val);
        [DllImport(LibFileName)]
        public static extern double GRTextFormatGetTextAngle(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRTextFormatSetTextAngle(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern TextOrientation GRTextFormatGetTextOrientation(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRTextFormatSetTextOrientation(IntPtr handle, TextOrientation val);
        [DllImport(LibFileName)]
        public static extern bool GRTextFormatGetWordWrap(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRTextFormatSetWordWrap(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRTextFormatClone(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRTextFormatFree(IntPtr Handle);

        //Picture原始API
        [DllImport(LibFileName)]
        public static extern int GRPictureGetHeight(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern PictureType GRPictureGetType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRPictureGetWidth(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPictureFree(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern bool GRPictureLoadFromBinary(IntPtr Handle, IntPtr BinaryObject);
        [DllImport(LibFileName)]
        public static extern bool GRPictureLoadFromFile(IntPtr Handle, string PathFile);
        [DllImport(LibFileName)]
        public static extern void GRPictureLoadFromMemory(IntPtr Handle, IntPtr Data, int DataLen);
        [DllImport(LibFileName)]
        public static extern bool GRPictureSaveToFile(IntPtr Handle, string PathFile);

        //BinaryObject原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRBinaryObjectGetAsBase64Text(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRBinaryObjectGetDataBuf(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRBinaryObjectGetDataSize(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBinaryObjectFree(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern bool GRBinaryObjectLoadFromBase64Text(IntPtr Handle, string Base64Text);
        [DllImport(LibFileName)]
        public static extern bool GRBinaryObjectLoadFromField(IntPtr Handle, IntPtr Field);
        [DllImport(LibFileName)]
        public static extern bool GRBinaryObjectLoadFromFile(IntPtr Handle, string PathFile);
        [DllImport(LibFileName)]
        public static extern bool GRBinaryObjectLoadFromMemory(IntPtr Handle, IntPtr pData, int ByteCount);
        [DllImport(LibFileName)]
        public static extern bool GRBinaryObjectSaveToFile(IntPtr Handle, string PathFile);

        //DateTime原始API
        [DllImport(LibFileName)]
        public static extern double GRDateTimeGetAsFloat(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDateTimeSetAsFloat(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern int GRDateTimeGetDay(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRDateTimeGetDayOfWeek(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRDateTimeGetDayOfYear(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRDateTimeGetHour(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRDateTimeGetMinute(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRDateTimeGetMonth(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRDateTimeGetSecond(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern long GRDateTimeGetValue(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDateTimeSetValue(IntPtr handle, long val);
        [DllImport(LibFileName)]
        public static extern int GRDateTimeGetYear(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRDateTimeFormat(IntPtr Handle, string Format);
        [DllImport(LibFileName)]
        public static extern void GRDateTimeFree(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRDateTimeValueFromDate(IntPtr Handle, int Year, int Month, int Day);
        [DllImport(LibFileName)]
        public static extern void GRDateTimeValueFromDateTime(IntPtr Handle, int Year, int Month, int Day, int Hour, int Minute, int Second);

        //Utility原始API
        [DllImport(LibFileName)]
        public static extern double GRUtilityGetCCPInnerX(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern double GRUtilityGetCCPInnerY(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern double GRUtilityGetCCPOuterX(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern double GRUtilityGetCCPOuterY(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRUtilityGetDisableExceptionMsg(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRUtilitySetDisableExceptionMsg(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRUtilityGetDisableHttpFailedMsg(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRUtilitySetDisableHttpFailedMsg(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRUtilityGetDisableNormalMsg(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRUtilitySetDisableNormalMsg(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern double GRUtilityGetShrinkFontMinWidthRatio(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRUtilitySetShrinkFontMinWidthRatio(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRUtilityGetTextLeftPunctuations(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRUtilitySetTextLeftPunctuations(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRUtilityGetTextRightPunctuations(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRUtilitySetTextRightPunctuations(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRUtilityGetTextWrapByWord(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRUtilitySetTextWrapByWord(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRUtilityGetTextWrapToJustify(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRUtilitySetTextWrapToJustify(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern void GRUtilityAlert(IntPtr Handle, string Msg);
        [DllImport(LibFileName)]
        public static extern void GRUtilityCalcCurveControlPoints(IntPtr Handle, double xFirst, double yFirst, double xMiddle, double yMiddle, double xAfter, double yAfter);
        [DllImport(LibFileName)]
        public static extern double GRUtilityCalcDrawFormatTextHeight(IntPtr Handle, string Text, double width, IntPtr TextFormat, IntPtr Font);
        [DllImport(LibFileName)]
        public static extern int GRUtilityCalcDrawFormatTextOutLen(IntPtr Handle, string Text, double Width, double Height, IntPtr TextFormat, IntPtr Font);
        [DllImport(LibFileName)]
        public static extern double GRUtilityCalcDrawFormatTextWidth(IntPtr Handle, string Text, IntPtr TextFormat, IntPtr Font);
        [DllImport(LibFileName)]
        public static extern double GRUtilityCalcLabelTextHeight(IntPtr Handle, string Text, IntPtr Font);
        [DllImport(LibFileName)]
        public static extern double GRUtilityCalcLabelTextWidth(IntPtr Handle, string Text, IntPtr Font);
        [DllImport(LibFileName)]
        public static extern int GRUtilityCalcTextOutLen(IntPtr Handle, double Width, double Height, string Text, bool WordWrap, IntPtr Font);
        [DllImport(LibFileName)]
        public static extern uint GRUtilityColorFromRGB(IntPtr Handle, int r, int g, int b);
        [DllImport(LibFileName)]
        public static extern IntPtr GRUtilityCreateBinaryObject(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRUtilityCreateDateTime(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern bool GRUtilityCreatePathOnNeed(IntPtr Handle, string Path);
        [DllImport(LibFileName)]
        public static extern IntPtr GRUtilityCreatePicture(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRUtilityCreateRecordset(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRUtilityCreateTextFormat(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRUtilityDateTimeFormat(IntPtr Handle, long Date, string Format);
        [DllImport(LibFileName)]
        public static extern void GRUtilityDisableDebug(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRUtilityExtractFileName(IntPtr Handle, string PathFile);
        [DllImport(LibFileName)]
        public static extern IntPtr GRUtilityExtractFilePath(IntPtr Handle, string PathFile);
        [DllImport(LibFileName)]
        public static extern int GRUtilityGetPrinterCount(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRUtilityGetPrinterName(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern int GRUtilityGetPrinterPaperCount(IntPtr Handle, string PrinterName);
        [DllImport(LibFileName)]
        public static extern IntPtr GRUtilityGetPrinterPaperName(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern IntPtr GRUtilityNumberFormat(IntPtr Handle, double Value, string Format);
        [DllImport(LibFileName)]
        public static extern IntPtr GRUtilityNumberFormatToBigHZ(IntPtr Handle, double Value, int Decimals);
        [DllImport(LibFileName)]
        public static extern IntPtr GRUtilityNumberFormatToBigHZAmount(IntPtr Handle, double Value);
        [DllImport(LibFileName)]
        public static extern double GRUtilityNumberRound45(IntPtr Handle, double Value, int Decimals);
        [DllImport(LibFileName)]
        public static extern double GRUtilityNumberRound465(IntPtr Handle, double Value, int Decimals);
        [DllImport(LibFileName)]
        public static extern bool GRUtilityPathFileExist(IntPtr Handle, string PathFile);
        [DllImport(LibFileName)]
        public static extern void GRUtilitySetMoneyLabelText(IntPtr Handle, string MoneyLabelText);

        //Graphics原始API
        [DllImport(LibFileName)]
        public static extern bool GRGraphicsGetByUnit(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsSetByUnit(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern double GRGraphicsGetHeight(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern double GRGraphicsGetLeft(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern double GRGraphicsGetLeftInPaper(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern double GRGraphicsGetTop(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern double GRGraphicsGetTopInPaper(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern double GRGraphicsGetWidth(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsArc(IntPtr Handle, double x, double y, double r, double BeginAngleDegree, double EndAngleDegree);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsBeginPath(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern double GRGraphicsCalcDrawFormatTextHeight(IntPtr Handle, string Text, double width, IntPtr TextFormat);
        [DllImport(LibFileName)]
        public static extern int GRGraphicsCalcDrawFormatTextOutLen(IntPtr Handle, string Text, double Width, double Height, IntPtr TextFormat);
        [DllImport(LibFileName)]
        public static extern double GRGraphicsCalcDrawFormatTextWidth(IntPtr Handle, string Text, IntPtr TextFormat);
        [DllImport(LibFileName)]
        public static extern double GRGraphicsCalcLabelTextHeight(IntPtr Handle, string Text);
        [DllImport(LibFileName)]
        public static extern double GRGraphicsCalcLabelTextWidth(IntPtr Handle, string Text);
        [DllImport(LibFileName)]
        public static extern int GRGraphicsCalcTextOutLen(IntPtr Handle, double Width, double Height, string Text, bool WordWrap);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsClosePath(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsCurveTo(IntPtr Handle, double xInnerControl, double yInnerControl, double xOuterControl, double yOuterControl, double xEnd, double yEnd);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsDrawFormatText(IntPtr Handle, string Text, double x, double y, double width, double height, IntPtr TextFormat);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsDrawFormatTextShrinkToFit(IntPtr Handle, string Text, double x, double y, double width, double height, IntPtr TextFormat);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsDrawLabelText(IntPtr Handle, string Text, double x, double y);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsDrawPicture(IntPtr Handle, IntPtr Picture, double left, double top, double width, double height, PictureAlignment PictureAlignment, PictureSizeMode PictureSizeMode, PictureTransparentMode Transparent);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsDrawPixel(IntPtr Handle, double x, double y, uint Color);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsDrawPointMarker(IntPtr Handle, PointMarkerStyle MarkerStyle, double cx, double cy, double size);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsDrawRotateText(IntPtr Handle, string Text, double x, double y, double TextAngle);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsDrawText(IntPtr Handle, string Text, double x, double y, double width, double Height, TextAlign TextAlign, bool WordWrap);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsEllipse(IntPtr Handle, double x, double y, double width, double height, bool ToFill);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsEllipseArc(IntPtr Handle, double left, double top, double width, double height, double BeginAngleDegree, double EndAngleDegree);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsEllipsePie(IntPtr Handle, double left, double top, double width, double height, double BeginAngleDegree, double EndAngleDegree, bool ToFill);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsEndPath(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsFillPath(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsFillRect(IntPtr Handle, double x, double y, double width, double height, uint FillColor);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsFontPointChangeTo(IntPtr Handle, double Point);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsFontPointRestore(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsGradientFillRect(IntPtr Handle, double x, double y, double width, double height, uint BeginColor, uint EndColor, GradientMode Mode);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsLineTo(IntPtr Handle, double x, double y);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsMoveTo(IntPtr Handle, double x, double y);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsPie(IntPtr Handle, double x, double y, double r, double BeginAngleDegree, double EndAngleDegree, bool ToFill);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsPopClipRect(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsPushClipRect(IntPtr Handle, double x, double y, double width, double height);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsRectangle(IntPtr Handle, double x, double y, double width, double height, bool ToFill);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsRestoreFillColor(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsRestoreFont(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsRestorePen(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsRestoreTextColor(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsRoundRect(IntPtr Handle, double x, double y, double width, double height, int cornerx, int cornery, bool ToFill);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsSelectFillColor(IntPtr Handle, uint BrushColor);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsSelectFont(IntPtr Handle, IntPtr Font);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsSelectPen(IntPtr Handle, double PenWidth, uint PenColor, PenStyle PenStyle);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsSelectTextColor(IntPtr Handle, uint TextColor);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsStrokeAndFillPath(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRGraphicsStrokePath(IntPtr Handle);

        //Object原始API
        [DllImport(LibFileName)]
        public static extern LockType GRObjectGetLock(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRObjectSetLock(IntPtr handle, LockType val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRObjectGetOwnerReport(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRObjectGetParent(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRObjectGetTag(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRObjectSetTag(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern void GRObjectAssign(IntPtr Handle, IntPtr Obj);

        //Parameter原始API
        [DllImport(LibFileName)]
        public static extern bool GRParameterGetAsBoolean(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRParameterSetAsBoolean(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern long GRParameterGetAsDateTime(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRParameterSetAsDateTime(IntPtr handle, long val);
        [DllImport(LibFileName)]
        public static extern double GRParameterGetAsFloat(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRParameterSetAsFloat(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern int GRParameterGetAsInteger(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRParameterSetAsInteger(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRParameterGetAsString(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRParameterSetAsString(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern ParameterDataType GRParameterGetDataType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRParameterSetDataType(IntPtr handle, ParameterDataType val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRParameterGetDisplayText(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRParameterGetFormat(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRParameterSetFormat(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRParameterGetIsNull(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRParameterGetName(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRParameterSetName(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRParameterGetValue(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRParameterSetValue(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern void GRParameterClear(IntPtr Handle);

        //Section原始API
        [DllImport(LibFileName)]
        public static extern uint GRSectionGetBackColor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSectionSetBackColor(IntPtr handle, uint val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRSectionGetBookmarkText(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSectionSetBookmarkText(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRSectionGetCanGrow(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSectionSetCanGrow(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRSectionGetCanShrink(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSectionSetCanShrink(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRSectionGetControls(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern DisplayCursor GRSectionGetCursor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSectionSetCursor(IntPtr handle, DisplayCursor val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRSectionGetFont(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRSectionGetFormatScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSectionSetFormatScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern double GRSectionGetHeight(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSectionSetHeight(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern bool GRSectionGetKeepTogether(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSectionSetKeepTogether(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern SectionType GRSectionGetSectionType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRSectionGetVisible(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSectionSetVisible(IntPtr handle, bool val);

        //CellBase原始API
        [DllImport(LibFileName)]
        public static extern uint GRCellBaseGetBackColor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCellBaseSetBackColor(IntPtr handle, uint val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRCellBaseGetBorder(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRCellBaseGetBorderCustom(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCellBaseSetBorderCustom(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRCellBaseGetCanGrow(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCellBaseSetCanGrow(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRCellBaseGetCanShrink(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCellBaseSetCanShrink(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRCellBaseGetControls(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern DisplayCursor GRCellBaseGetCursor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCellBaseSetCursor(IntPtr handle, DisplayCursor val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRCellBaseGetDisplayText(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCellBaseSetDisplayText(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRCellBaseGetFont(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern uint GRCellBaseGetForeColor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCellBaseSetForeColor(IntPtr handle, uint val);
        [DllImport(LibFileName)]
        public static extern bool GRCellBaseGetFreeCell(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCellBaseSetFreeCell(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRCellBaseGetGetDisplayTextScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCellBaseSetGetDisplayTextScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern double GRCellBaseGetPaddingBottom(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCellBaseSetPaddingBottom(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRCellBaseGetPaddingLeft(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCellBaseSetPaddingLeft(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRCellBaseGetPaddingRight(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCellBaseSetPaddingRight(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRCellBaseGetPaddingTop(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCellBaseSetPaddingTop(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern PrintType GRCellBaseGetPrintType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCellBaseSetPrintType(IntPtr handle, PrintType val);
        [DllImport(LibFileName)]
        public static extern bool GRCellBaseGetShowMoneyLabel(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCellBaseSetShowMoneyLabel(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRCellBaseGetShowMoneyLine(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCellBaseSetShowMoneyLine(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRCellBaseGetShrinkFontToFit(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCellBaseSetShrinkFontToFit(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRCellBaseGetTextFormat(IntPtr handle);

        //Control原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetAlignColumn(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetAlignColumn(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetAlignColumnEx(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetAlignColumnEx(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern AlignColumnStyle GRControlGetAlignColumnSide(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetAlignColumnSide(IntPtr handle, AlignColumnStyle val);
        [DllImport(LibFileName)]
        public static extern int GRControlGetAnchor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetAnchor(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetAsBarcode(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetAsChart(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetAsFieldBox(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetAsFreeGrid(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetAsLine(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetAsMemoBox(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetAsPictureBox(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetAsRichTextBox(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetAsShapeBox(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetAsStaticBox(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetAsSubReport(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetAsSummaryBox(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetAsSystemVarBox(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetAsTextBox(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern uint GRControlGetBackColor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetBackColor(IntPtr handle, uint val);
        [DllImport(LibFileName)]
        public static extern BackStyle GRControlGetBackStyle(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetBackStyle(IntPtr handle, BackStyle val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetBorder(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern CenterStyle GRControlGetCenter(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetCenter(IntPtr handle, CenterStyle val);
        [DllImport(LibFileName)]
        public static extern ControlType GRControlGetControlType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern DisplayCursor GRControlGetCursor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetCursor(IntPtr handle, DisplayCursor val);
        [DllImport(LibFileName)]
        public static extern bool GRControlGetCustomDraw(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetCustomDraw(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetCustomDrawScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetCustomDrawScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern DockStyle GRControlGetDock(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetDock(IntPtr handle, DockStyle val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetFont(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern uint GRControlGetForeColor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetForeColor(IntPtr handle, uint val);
        [DllImport(LibFileName)]
        public static extern double GRControlGetHeight(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetHeight(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRControlGetLeft(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetLeft(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlGetName(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetName(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern double GRControlGetPaddingBottom(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetPaddingBottom(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRControlGetPaddingLeft(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetPaddingLeft(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRControlGetPaddingRight(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetPaddingRight(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRControlGetPaddingTop(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetPaddingTop(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern PrintType GRControlGetPrintType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetPrintType(IntPtr handle, PrintType val);
        [DllImport(LibFileName)]
        public static extern ShiftMode GRControlGetShiftMode(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetShiftMode(IntPtr handle, ShiftMode val);
        [DllImport(LibFileName)]
        public static extern double GRControlGetTop(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetTop(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern bool GRControlGetVisible(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetVisible(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern double GRControlGetWidth(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetWidth(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern void GRControlBringToFront(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRControlDrawDefault(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern bool GRControlGetAnchorSide(IntPtr Handle, AnchorStyles Side);
        [DllImport(LibFileName)]
        public static extern void GRControlSendToBack(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRControlSetAnchorSide(IntPtr Handle, AnchorStyles Side, bool ToAnchor);
        [DllImport(LibFileName)]
        public static extern void GRControlSetBounds(IntPtr Handle, double left, double Top, double Right, double Bottom);

        //TextBox原始API
        [DllImport(LibFileName)]
        public static extern bool GRTextBoxGetCanGrow(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRTextBoxSetCanGrow(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRTextBoxGetCanShrink(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRTextBoxSetCanShrink(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRTextBoxGetDisplayText(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRTextBoxSetDisplayText(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRTextBoxGetGetDisplayTextScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRTextBoxSetGetDisplayTextScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRTextBoxGetShowMoneyLabel(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRTextBoxSetShowMoneyLabel(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRTextBoxGetShowMoneyLine(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRTextBoxSetShowMoneyLine(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRTextBoxGetShrinkFontToFit(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRTextBoxSetShrinkFontToFit(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRTextBoxGetTextFormat(IntPtr handle);

        //StaticBox原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRStaticBoxGetParameter(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRStaticBoxSetParameter(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRStaticBoxGetText(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRStaticBoxSetText(IntPtr handle, string val);

        //FieldBox原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRFieldBoxGetDataField(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFieldBoxSetDataField(IntPtr handle, string val);

        //SummaryBox原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRSummaryBoxGetDataField(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSummaryBoxSetDataField(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRSummaryBoxGetDisplayField(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSummaryBoxSetDisplayField(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRSummaryBoxGetFormat(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSummaryBoxSetFormat(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern int GRSummaryBoxGetRankNo(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSummaryBoxSetRankNo(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern SummaryFun GRSummaryBoxGetSummaryFun(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSummaryBoxSetSummaryFun(IntPtr handle, SummaryFun val);
        [DllImport(LibFileName)]
        public static extern double GRSummaryBoxGetValue(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSummaryBoxSetValue(IntPtr handle, double val);

        //SystemVarBox原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRSystemVarBoxGetFormat(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSystemVarBoxSetFormat(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern int GRSystemVarBoxGetGroupIndex(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSystemVarBoxSetGroupIndex(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern SystemVarType GRSystemVarBoxGetSystemVar(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSystemVarBoxSetSystemVar(IntPtr handle, SystemVarType val);

        //MemoBox原始API
        [DllImport(LibFileName)]
        public static extern double GRMemoBoxGetAsFloat(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRMemoBoxGetFlowTo(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRMemoBoxSetFlowTo(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRMemoBoxGetText(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRMemoBoxSetText(IntPtr handle, string val);

        //ShapeBox原始API
        [DllImport(LibFileName)]
        public static extern int GRShapeBoxGetCornerDx(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRShapeBoxSetCornerDx(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern int GRShapeBoxGetCornerDy(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRShapeBoxSetCornerDy(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern uint GRShapeBoxGetFillColor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRShapeBoxSetFillColor(IntPtr handle, uint val);
        [DllImport(LibFileName)]
        public static extern BackStyle GRShapeBoxGetFillStyle(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRShapeBoxSetFillStyle(IntPtr handle, BackStyle val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRShapeBoxGetLinePen(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern ShapeType GRShapeBoxGetShapeType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRShapeBoxSetShapeType(IntPtr handle, ShapeType val);

        //PictureBox原始API
        [DllImport(LibFileName)]
        public static extern PictureAlignment GRPictureBoxGetAlignment(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPictureBoxSetAlignment(IntPtr handle, PictureAlignment val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRPictureBoxGetDataField(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPictureBoxSetDataField(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRPictureBoxGetImageFile(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPictureBoxSetImageFile(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern int GRPictureBoxGetImageIndex(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPictureBoxSetImageIndex(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRPictureBoxGetPicture(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPictureBoxSetPicture(IntPtr handle, IntPtr val);
        [DllImport(LibFileName)]
        public static extern PictureRotateMode GRPictureBoxGetRotateMode(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPictureBoxSetRotateMode(IntPtr handle, PictureRotateMode val);
        [DllImport(LibFileName)]
        public static extern PictureSizeMode GRPictureBoxGetSizeMode(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPictureBoxSetSizeMode(IntPtr handle, PictureSizeMode val);
        [DllImport(LibFileName)]
        public static extern PictureTransparentMode GRPictureBoxGetTransparentMode(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPictureBoxSetTransparentMode(IntPtr handle, PictureTransparentMode val);
        [DllImport(LibFileName)]
        public static extern void GRPictureBoxAttachSystemImage(IntPtr Handle, SystemImage SystemImage);
        [DllImport(LibFileName)]
        public static extern bool GRPictureBoxLoadFromBinary(IntPtr Handle, IntPtr BinaryObject);
        [DllImport(LibFileName)]
        public static extern bool GRPictureBoxLoadFromFile(IntPtr Handle, string PathFile);
        [DllImport(LibFileName)]
        public static extern bool GRPictureBoxLoadFromMemory(IntPtr Handle, IntPtr pBuffer, int BytesCount);

        //RichTextBox原始API
        [DllImport(LibFileName)]
        public static extern bool GRRichTextBoxGetCanGrow(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRRichTextBoxSetCanGrow(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRRichTextBoxGetCanShrink(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRRichTextBoxSetCanShrink(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRRichTextBoxGetDataField(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRRichTextBoxSetDataField(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRRichTextBoxGetRTF(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRRichTextBoxSetRTF(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRRichTextBoxLoadFromFile(IntPtr Handle, string PathFile);
        [DllImport(LibFileName)]
        public static extern bool GRRichTextBoxLoadFromMemory(IntPtr Handle, IntPtr pBuffer, int BytesCount);

        //SubReport原始API
        [DllImport(LibFileName)]
        public static extern bool GRSubReportGetCanGrow(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSubReportSetCanGrow(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRSubReportGetCanShrink(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSubReportSetCanShrink(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRSubReportGetHideOnRecordsetEmpty(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSubReportSetHideOnRecordsetEmpty(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRSubReportGetParentPageSettings(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSubReportSetParentPageSettings(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRSubReportGetRelationFields(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSubReportSetRelationFields(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRSubReportGetReport(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRSubReportGetReportFile(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSubReportSetReportFile(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRSubReportGetResetPageNumber(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSubReportSetResetPageNumber(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRSubReportGetToNewExcelSheet(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRSubReportSetToNewExcelSheet(IntPtr handle, bool val);

        //Line原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRLineGetLinePen(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern double GRLineGetX1(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRLineSetX1(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRLineGetX2(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRLineSetX2(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRLineGetY1(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRLineSetY1(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRLineGetY2(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRLineSetY2(IntPtr handle, double val);

        //Barcode原始API
        [DllImport(LibFileName)]
        public static extern StringAlignment GRBarcodeGetAlignment(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetAlignment(IntPtr handle, StringAlignment val);
        [DllImport(LibFileName)]
        public static extern BarcodeType GRBarcodeGetBarcodeType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetBarcodeType(IntPtr handle, BarcodeType val);
        [DllImport(LibFileName)]
        public static extern int GRBarcodeGetBarDPI(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetBarDPI(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern double GRBarcodeGetBarRatio(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetBarRatio(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRBarcodeGetBarWidth(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetBarWidth(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern StringAlignment GRBarcodeGetCaptionAlignment(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetCaptionAlignment(IntPtr handle, StringAlignment val);
        [DllImport(LibFileName)]
        public static extern BarcodeCaptionPosition GRBarcodeGetCaptionPosition(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetCaptionPosition(IntPtr handle, BarcodeCaptionPosition val);
        [DllImport(LibFileName)]
        public static extern bool GRBarcodeGetCheckSum(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetCheckSum(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern BarcodeDirection GRBarcodeGetDirection(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetDirection(IntPtr handle, BarcodeDirection val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRBarcodeGetDisplayText(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern DtmxEncoding GRBarcodeGetDtmxEncoding(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetDtmxEncoding(IntPtr handle, DtmxEncoding val);
        [DllImport(LibFileName)]
        public static extern DtmxMatrixSize GRBarcodeGetDtmxMatrixSize(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetDtmxMatrixSize(IntPtr handle, DtmxMatrixSize val);
        [DllImport(LibFileName)]
        public static extern int GRBarcodeGetPDF417Columns(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetPDF417Columns(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern int GRBarcodeGetPDF417ErrorLevel(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetPDF417ErrorLevel(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern int GRBarcodeGetPDF417Rows(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetPDF417Rows(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern bool GRBarcodeGetPDF417Simple(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetPDF417Simple(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern int GRBarcodeGetQRCodeErrorLevel(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetQRCodeErrorLevel(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern int GRBarcodeGetQRCodeMask(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetQRCodeMask(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern int GRBarcodeGetQRCodeVersion(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetQRCodeVersion(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRBarcodeGetText(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRBarcodeSetText(IntPtr handle, string val);

        //ChartAxis原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartAxisGetCoordLinePen(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRChartAxisGetCoordLineVisible(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartAxisSetCoordLineVisible(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRChartAxisGetDateTimeAxis(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartAxisSetDateTimeAxis(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartAxisGetLabel(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartAxisSetLabel(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartAxisGetLinePen(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRChartAxisGetLineVisible(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartAxisSetLineVisible(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern int GRChartAxisGetMarginBeginWeight(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartAxisSetMarginBeginWeight(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern int GRChartAxisGetMarginEndWeight(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartAxisSetMarginEndWeight(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern double GRChartAxisGetMax(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartAxisSetMax(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern bool GRChartAxisGetMaxAtScale(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartAxisSetMaxAtScale(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern double GRChartAxisGetMin(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartAxisSetMin(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern int GRChartAxisGetScaleCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern double GRChartAxisGetSpace(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartAxisSetSpace(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRChartAxisGetTextAngle(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartAxisSetTextAngle(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartAxisGetTextFormat(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartAxisSetTextFormat(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRChartAxisGetTextVisible(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartAxisSetTextVisible(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern void GRChartAxisAddCustomCoordLine(IntPtr Handle, double PosVal, string Text, double LineWidth, uint LineColor, PenStyle LineStyle);
        [DllImport(LibFileName)]
        public static extern void GRChartAxisAddCustomScale(IntPtr Handle, double ScaleValue, string ScaleText);
        [DllImport(LibFileName)]
        public static extern void GRChartAxisEmptyCustomCoordLine(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRChartAxisEmptyCustomScale(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartAxisGetScaleText(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern double GRChartAxisGetScaleValue(IntPtr Handle, int Index);

        //ChartSeries原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartSeriesGetBorderPen(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRChartSeriesGetByY2Axis(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriesSetByY2Axis(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern ChartType GRChartSeriesGetChartType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriesSetChartType(IntPtr handle, ChartType val);
        [DllImport(LibFileName)]
        public static extern uint GRChartSeriesGetFillColor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriesSetFillColor(IntPtr handle, uint val);
        [DllImport(LibFileName)]
        public static extern bool GRChartSeriesGetFillColorAuto(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriesSetFillColorAuto(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRChartSeriesGetLabelAsGroup(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriesSetLabelAsGroup(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRChartSeriesGetLabelInBar(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriesSetLabelInBar(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartSeriesGetLabelText(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriesSetLabelText(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern double GRChartSeriesGetLabelTextAngle(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriesSetLabelTextAngle(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern uint GRChartSeriesGetMarkerColor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriesSetMarkerColor(IntPtr handle, uint val);
        [DllImport(LibFileName)]
        public static extern bool GRChartSeriesGetMarkerColorAuto(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriesSetMarkerColorAuto(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartSeriesGetMarkerPen(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRChartSeriesGetMarkerSize(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriesSetMarkerSize(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern PointMarkerStyle GRChartSeriesGetMarkerStyle(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriesSetMarkerStyle(IntPtr handle, PointMarkerStyle val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartSeriesGetTooltipText(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriesSetTooltipText(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartSeriesGetValueFormat(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriesSetValueFormat(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartSeriesGetXValueField(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriesSetXValueField(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartSeriesGetYValueField(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriesSetYValueField(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartSeriesGetZValueField(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriesSetZValueField(IntPtr handle, string val);

        //Chart原始API
        [DllImport(LibFileName)]
        public static extern int GRChartGetBarWidthPercent(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetBarWidthPercent(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern double GRChartGetBubbleScalePerCm(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetBubbleScalePerCm(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern bool GRChartGetChart3D(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetChart3D(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern int GRChartGetFillColorCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRChartGetGroupAuto(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetGroupAuto(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern int GRChartGetGroupCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetGroupCount(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartGetGroupField(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetGroupField(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRChartGetLegendAtBottom(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetLegendAtBottom(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartGetLegendBorderPen(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRChartGetLegendColumnCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetLegendColumnCount(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern DisplayCursor GRChartGetLegendCursor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetLegendCursor(IntPtr handle, DisplayCursor val);
        [DllImport(LibFileName)]
        public static extern bool GRChartGetLegendShowSum(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetLegendShowSum(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartGetLegendSumLabel(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetLegendSumLabel(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRChartGetLegendValueVisible(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetLegendValueVisible(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRChartGetLegendVisible(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetLegendVisible(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartGetRecordset(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartGetSeries(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRChartGetSeriesAuto(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetSeriesAuto(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern int GRChartGetSeriesCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetSeriesCount(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern DisplayCursor GRChartGetSeriesCursor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetSeriesCursor(IntPtr handle, DisplayCursor val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartGetSeriesField(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetSeriesField(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRChartGetSingleSeriesColored(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetSingleSeriesColored(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartGetTitle(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSetTitle(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartGetTitleFont(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartGetValueFont(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartGetXAxis(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartGetY2Axis(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartGetYAxis(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRChartAddFillColor(IntPtr Handle, uint FillColor);
        [DllImport(LibFileName)]
        public static extern void GRChartAddXYValue(IntPtr Handle, int SeriesIndex, double XVal, double YVal);
        [DllImport(LibFileName)]
        public static extern void GRChartAddXYZValue(IntPtr Handle, int SeriesIndex, double XVal, double YVal, double ZVal);
        [DllImport(LibFileName)]
        public static extern void GRChartEmptyFillColors(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRChartEmptyValues(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern uint GRChartFillColor(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartGroupLabel(IntPtr Handle, int GroupIndex);
        [DllImport(LibFileName)]
        public static extern bool GRChartLoadDataFromXML(IntPtr Handle, string XML, bool FirstSeries, bool AutoSeries, bool AutoGroup);
        [DllImport(LibFileName)]
        public static extern bool GRChartLoadXYDataFromXML(IntPtr Handle, string XML, bool AutoSeries);
        [DllImport(LibFileName)]
        public static extern bool GRChartLoadXYZDataFromXML(IntPtr Handle, string XML, bool AutoSeries);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartSeriesLabel(IntPtr Handle, int SeriesIndex);
        [DllImport(LibFileName)]
        public static extern void GRChartSetGroupLabel(IntPtr Handle, int GroupIndex, string Text);
        [DllImport(LibFileName)]
        public static extern void GRChartSetSeriesLabel(IntPtr Handle, int Index, string Text);
        [DllImport(LibFileName)]
        public static extern void GRChartSetValue(IntPtr Handle, int SeriesIndex, int GroupIndex, double Value);
        [DllImport(LibFileName)]
        public static extern double GRChartValue(IntPtr Handle, int SeriesIndex, int GroupIndex);
        [DllImport(LibFileName)]
        public static extern double GRChartXValue(IntPtr Handle, int SeriesIndex, int Index);
        [DllImport(LibFileName)]
        public static extern int GRChartXYZValueCount(IntPtr Handle, int SeriesIndex);
        [DllImport(LibFileName)]
        public static extern double GRChartYValue(IntPtr Handle, int SeriesIndex, int Index);
        [DllImport(LibFileName)]
        public static extern double GRChartZValue(IntPtr Handle, int SeriesIndex, int Index);

        //FreeGridCell原始API
        [DllImport(LibFileName)]
        public static extern int GRFreeGridCellGetColSpan(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFreeGridCellSetColSpan(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern int GRFreeGridCellGetColumnIndex(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFreeGridCellGetDataName(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFreeGridCellSetDataName(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFreeGridCellGetFreeGrid(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRFreeGridCellGetRowIndex(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRFreeGridCellGetRowSpan(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFreeGridCellSetRowSpan(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFreeGridCellGetText(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFreeGridCellSetText(IntPtr handle, string val);

        //FreeGridColumn原始API
        [DllImport(LibFileName)]
        public static extern int GRFreeGridColumnGetColumnIndex(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFreeGridColumnGetFreeGrid(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRFreeGridColumnGetVisible(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFreeGridColumnSetVisible(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern double GRFreeGridColumnGetWidth(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFreeGridColumnSetWidth(IntPtr handle, double val);

        //FreeGridRow原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRFreeGridRowGetFreeGrid(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRFreeGridRowGetRowIndex(IntPtr handle);

        //FreeGrid原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRFreeGridGetColLinePen(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRFreeGridGetColumnCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFreeGridSetColumnCount(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern double GRFreeGridGetDesignHeight(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern double GRFreeGridGetDesignWidth(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRFreeGridGetGrowToBottom(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFreeGridSetGrowToBottom(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern int GRFreeGridGetRowCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFreeGridSetRowCount(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFreeGridGetRowLinePen(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRFreeGridGetShowColLine(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFreeGridSetShowColLine(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRFreeGridGetShowRowLine(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFreeGridSetShowRowLine(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRFreeGridGetTitleRepeat(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFreeGridSetTitleRepeat(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern int GRFreeGridGetTitleRows(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFreeGridSetTitleRows(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFreeGridCellAt(IntPtr Handle, int RowIndex, int ColumnIndex);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFreeGridCellByDataName(IntPtr Handle, string DataName);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFreeGridColumnAt(IntPtr Handle, int ColumnIndex);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFreeGridRowAt(IntPtr Handle, int RowIndex);

        //Field原始API
        [DllImport(LibFileName)]
        public static extern bool GRFieldGetAsBoolean(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFieldSetAsBoolean(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern long GRFieldGetAsCurrency(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFieldSetAsCurrency(IntPtr handle, long val);
        [DllImport(LibFileName)]
        public static extern long GRFieldGetAsDateTime(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFieldSetAsDateTime(IntPtr handle, long val);
        [DllImport(LibFileName)]
        public static extern double GRFieldGetAsFloat(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFieldSetAsFloat(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern int GRFieldGetAsInteger(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFieldSetAsInteger(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFieldGetAsString(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFieldSetAsString(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFieldGetDataBuffer(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRFieldGetDataSize(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFieldGetDBFieldName(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFieldSetDBFieldName(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFieldGetDisplayText(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFieldSetDisplayText(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern FieldType GRFieldGetFieldType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFieldSetFieldType(IntPtr handle, FieldType val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFieldGetFormat(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFieldSetFormat(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFieldGetGetDisplayTextScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFieldSetGetDisplayTextScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRFieldGetIsNull(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFieldGetName(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFieldSetName(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRFieldGetRTrimBlankChars(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFieldSetRTrimBlankChars(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFieldGetValue(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRFieldSetValue(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern void GRFieldClear(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRFieldLoadFromBinary(IntPtr Handle, IntPtr BinaryObject);
        [DllImport(LibFileName)]
        public static extern void GRFieldLoadFromFile(IntPtr Handle, string PathFile);
        [DllImport(LibFileName)]
        public static extern void GRFieldLoadFromMemory(IntPtr Handle, IntPtr pBuffer, int ByteCount);

        //Recordset原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRRecordsetGetBeforePostRecordScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetSetBeforePostRecordScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRRecordsetGetConnectionString(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetSetConnectionString(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRRecordsetGetEditable(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetSetEditable(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRRecordsetGetFetchRecordScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetSetFetchRecordScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRRecordsetGetFields(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRRecordsetGetPageProcessRecordScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetSetPageProcessRecordScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRRecordsetGetProcessRecordScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetSetProcessRecordScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRRecordsetGetQuerySQL(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetSetQuerySQL(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern int GRRecordsetGetRecordCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRRecordsetGetRecordNo(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRRecordsetGetSkipQuery(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetSetSkipQuery(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRRecordsetGetSortAsc(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetSetSortAsc(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRRecordsetGetSortCaseSensitive(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetSetSortCaseSensitive(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRRecordsetGetSortFields(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetSetSortFields(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRRecordsetGetXmlTableName(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetSetXmlTableName(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRRecordsetAddField(IntPtr Handle, string Name, FieldType Type);
        [DllImport(LibFileName)]
        public static extern bool GRRecordsetAppend(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern bool GRRecordsetBof(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetCancel(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern bool GRRecordsetDuplicate(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern bool GRRecordsetEdit(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetEmpty(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern bool GRRecordsetEof(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetFilterBegin(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetFilterByCount(IntPtr Handle, int MaxCount);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetFilterByStep(IntPtr Handle, int Step, bool AlwaysLast);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetFilterEnd(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetFilterReset(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetFilterSelect(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetFirst(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetFree(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetLast(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern bool GRRecordsetLoadDataFromXML(IntPtr Handle, string DataText);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetMoveBy(IntPtr Handle, int Delta);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetMoveTo(IntPtr Handle, int RecordNo);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetNext(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern bool GRRecordsetPost(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetPrior(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRRecordsetResort(IntPtr Handle, string Fields, bool Ascending, bool CaseSensitive);
        [DllImport(LibFileName)]
        public static extern IntPtr GRRecordsetSaveDataToXML(IntPtr Handle);

        //Column原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnGetContentCell(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnGetDetailGrid(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRColumnGetFixedWidth(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRColumnSetFixedWidth(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnGetName(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRColumnSetName(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnGetTitleCell(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRColumnGetVisible(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRColumnSetVisible(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern double GRColumnGetWidth(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRColumnSetWidth(IntPtr handle, double val);

        //ColumnCell原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnCellGetColumn(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnCellGetName(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRColumnCellSetName(IntPtr handle, string val);

        //ColumnContentCell原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnContentCellGetColumnContent(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnContentCellGetDataField(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRColumnContentCellSetDataField(IntPtr handle, string val);

        //ColumnTitleCell原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnTitleCellGetColumnTitle(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRColumnTitleCellGetGroupTitle(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern double GRColumnTitleCellGetHeight(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRColumnTitleCellSetHeight(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnTitleCellGetSubTitles(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnTitleCellGetSupCell(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnTitleCellGetText(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRColumnTitleCellSetText(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRColumnTitleCellGetVisible(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRColumnTitleCellSetVisible(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnTitleCellAddSubGroupTitle(IntPtr Handle, string Name, string Title);
        [DllImport(LibFileName)]
        public static extern void GRColumnTitleCellEncloseColumn(IntPtr Handle, string ColumnName);

        //ColumnSection原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnSectionGetDetailGrid(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRColumnSectionSetCellsBackColor(IntPtr Handle, uint BackColor);
        [DllImport(LibFileName)]
        public static extern void GRColumnSectionSetCellsForeColor(IntPtr Handle, uint ForeColor);

        //ColumnContent原始API
        [DllImport(LibFileName)]
        public static extern bool GRColumnContentGetAdjustRowHeight(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRColumnContentSetAdjustRowHeight(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern uint GRColumnContentGetAlternatingBackColor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRColumnContentSetAlternatingBackColor(IntPtr handle, uint val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnContentGetContentCells(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRColumnContentGetGrowToNextRow(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRColumnContentSetGrowToNextRow(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRColumnContentGetRowsIncludeGroup(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRColumnContentSetRowsIncludeGroup(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern int GRColumnContentGetRowsPerPage(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRColumnContentSetRowsPerPage(IntPtr handle, int val);

        //ColumnTitle原始API
        [DllImport(LibFileName)]
        public static extern bool GRColumnTitleGetBeforeHeaders(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRColumnTitleSetBeforeHeaders(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern RepeatStyle GRColumnTitleGetRepeatStyle(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRColumnTitleSetRepeatStyle(IntPtr handle, RepeatStyle val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnTitleGetTitleCells(IntPtr handle);

        //GroupSection原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRGroupSectionGetGroup(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRGroupSectionGetHNewPageFixed(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupSectionSetHNewPageFixed(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern NewPageStyle GRGroupSectionGetNewPage(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupSectionSetNewPage(IntPtr handle, NewPageStyle val);
        [DllImport(LibFileName)]
        public static extern NewPageColumnStyle GRGroupSectionGetNewPageColumn(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupSectionSetNewPageColumn(IntPtr handle, NewPageColumnStyle val);
        [DllImport(LibFileName)]
        public static extern bool GRGroupSectionGetPrintGridBorder(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupSectionSetPrintGridBorder(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRGroupSectionGetRepeatOnPage(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupSectionSetRepeatOnPage(IntPtr handle, bool val);

        //GroupHeader原始API
        [DllImport(LibFileName)]
        public static extern GrpKpTogetherStyle GRGroupHeaderGetGroupKeepTogether(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupHeaderSetGroupKeepTogether(IntPtr handle, GrpKpTogetherStyle val);
        [DllImport(LibFileName)]
        public static extern bool GRGroupHeaderGetIncludeFooter(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupHeaderSetIncludeFooter(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRGroupHeaderGetOccupiedColumns(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupHeaderSetOccupiedColumns(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRGroupHeaderGetOccupyColumn(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupHeaderSetOccupyColumn(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRGroupHeaderGetSameAsColumn(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupHeaderSetSameAsColumn(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern OCGroupHeaderVAlign GRGroupHeaderGetVAlign(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupHeaderSetVAlign(IntPtr handle, OCGroupHeaderVAlign val);

        //GroupFooter原始API
        [DllImport(LibFileName)]
        public static extern bool GRGroupFooterGetAppendBlankRowExclude(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupFooterSetAppendBlankRowExclude(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRGroupFooterGetPrintAtBottom(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupFooterSetPrintAtBottom(IntPtr handle, bool val);

        //Group原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRGroupGetByFields(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupSetByFields(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRGroupGetDetailGrid(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRGroupGetFooter(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRGroupGetGroupBeginScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupSetGroupBeginScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRGroupGetGroupEndScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupSetGroupEndScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRGroupGetHeader(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRGroupGetLimitsPerPage(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupSetLimitsPerPage(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRGroupGetName(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupSetName(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRGroupGetPageGroup(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupSetPageGroup(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRGroupGetSortAsc(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupSetSortAsc(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRGroupGetSortSummaryBox(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupSetSortSummaryBox(IntPtr handle, string val);

        //CrossTab原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRCrossTabGetBeginDateParameter(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetBeginDateParameter(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRCrossTabGetEndDateParameter(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetEndDateParameter(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRCrossTabGetGroupAutoSum(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetGroupAutoSum(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRCrossTabGetHCrossFields(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetHCrossFields(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern PeriodType GRCrossTabGetHCrossPeriodType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetHCrossPeriodType(IntPtr handle, PeriodType val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRCrossTabGetHPercentColumns(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetHPercentColumns(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRCrossTabGetHResort(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetHResort(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRCrossTabGetHSortAsc(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetHSortAsc(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRCrossTabGetHTotalAtFirst(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetHTotalAtFirst(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern int GRCrossTabGetListCols(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetListCols(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRCrossTabGetPercentFormat(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetPercentFormat(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRCrossTabGetRecordset(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRCrossTabGetSubtotalCols(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetSubtotalCols(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern int GRCrossTabGetTotalCols(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetTotalCols(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRCrossTabGetTotalExcludeColumns(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetTotalExcludeColumns(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRCrossTabGetTotalHPercentColumns(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetTotalHPercentColumns(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRCrossTabGetTotalVPercentColumns(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetTotalVPercentColumns(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRCrossTabGetVCrossFields(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetVCrossFields(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRCrossTabGetVPercentColumns(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetVPercentColumns(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRCrossTabGetVResort(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetVResort(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRCrossTabGetVSortAsc(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabSetVSortAsc(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern long GRCrossTabGetCurPeriodBeginDate(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern long GRCrossTabGetCurPeriodEndDate(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabHBeginAddItem(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabHEndAddItem(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabVBeginAddItem(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRCrossTabVEndAddItem(IntPtr Handle);

        //DetailGrid原始API
        [DllImport(LibFileName)]
        public static extern bool GRDetailGridGetAppendBlankCol(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetAppendBlankCol(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern double GRDetailGridGetAppendBlankColWidth(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetAppendBlankColWidth(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern bool GRDetailGridGetAppendBlankRow(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetAppendBlankRow(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRDetailGridGetAppendBlankRowAtLast(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetAppendBlankRowAtLast(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern uint GRDetailGridGetBackColor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetBackColor(IntPtr handle, uint val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRDetailGridGetBorder(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern PrintType GRDetailGridGetBorderPrintType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetBorderPrintType(IntPtr handle, PrintType val);
        [DllImport(LibFileName)]
        public static extern bool GRDetailGridGetCenterView(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetCenterView(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRDetailGridGetColLinePen(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRDetailGridGetColumnContent(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRDetailGridGetColumns(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRDetailGridGetColumnTitle(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRDetailGridGetCrossTab(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRDetailGridGetFixCols(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetFixCols(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRDetailGridGetFont(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern PrintType GRDetailGridGetGridLinePrintType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetGridLinePrintType(IntPtr handle, PrintType val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRDetailGridGetGroups(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRDetailGridGetGrowToBottom(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetGrowToBottom(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRDetailGridGetIsCrossTab(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetIsCrossTab(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern NewPageStyle GRDetailGridGetNewPage(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetNewPage(IntPtr handle, NewPageStyle val);
        [DllImport(LibFileName)]
        public static extern int GRDetailGridGetPageColumnCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetPageColumnCount(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern PageColumnDirection GRDetailGridGetPageColumnDirection(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetPageColumnDirection(IntPtr handle, PageColumnDirection val);
        [DllImport(LibFileName)]
        public static extern bool GRDetailGridGetPageColumnGroupNA(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetPageColumnGroupNA(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern double GRDetailGridGetPageColumnSpacing(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetPageColumnSpacing(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern bool GRDetailGridGetPrintAdaptFitText(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetPrintAdaptFitText(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern ColumnPrintAdaptMethod GRDetailGridGetPrintAdaptMethod(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetPrintAdaptMethod(IntPtr handle, ColumnPrintAdaptMethod val);
        [DllImport(LibFileName)]
        public static extern bool GRDetailGridGetPrintAdaptRepeat(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetPrintAdaptRepeat(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern int GRDetailGridGetPrintAdaptRFCStep(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetPrintAdaptRFCStep(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern bool GRDetailGridGetPrintAdaptTryToOnePage(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetPrintAdaptTryToOnePage(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRDetailGridGetRecordset(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRDetailGridGetRowLinePen(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRDetailGridGetShowColLine(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetShowColLine(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRDetailGridGetShowRowLine(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridSetShowRowLine(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRDetailGridAddColumn(IntPtr Handle, string Name, string Title, string DataField, double Width);
        [DllImport(LibFileName)]
        public static extern IntPtr GRDetailGridAddGroupTitle(IntPtr Handle, string Name, string Title);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridClearColumns(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridClearGroupTitles(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRDetailGridColumnByShowOrder(IntPtr Handle, int OrderNo);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridColumnMoveTo(IntPtr Handle, string ColumnName, string NewGroupTitleCellName, int NewShowOrderNo);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridColumnMoveToEnd(IntPtr Handle, string ColumnName);
        [DllImport(LibFileName)]
        public static extern IntPtr GRDetailGridFindGroupTitleCell(IntPtr Handle, string Name);
        [DllImport(LibFileName)]
        public static extern void GRDetailGridStartNewGroup(IntPtr Handle, int Index);

        //ReportSection原始API
        [DllImport(LibFileName)]
        public static extern bool GRReportSectionGetCenterWithDetailGrid(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSectionSetCenterWithDetailGrid(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportSectionGetName(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSectionSetName(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern NewPageStyle GRReportSectionGetNewPage(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSectionSetNewPage(IntPtr handle, NewPageStyle val);
        [DllImport(LibFileName)]
        public static extern bool GRReportSectionGetRepeatOnPage(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSectionSetRepeatOnPage(IntPtr handle, bool val);

        //PageHeader原始API

        //PageFooter原始API

        //ReportHeader原始API

        //ReportFooter原始API
        [DllImport(LibFileName)]
        public static extern bool GRReportFooterGetAppendBlankRowExclude(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportFooterSetAppendBlankRowExclude(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRReportFooterGetPrintAtBottom(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportFooterSetPrintAtBottom(IntPtr handle, bool val);

        //Controls原始API
        [DllImport(LibFileName)]
        public static extern int GRControlsGetCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlsAdd(IntPtr Handle, ControlType Type);
        [DllImport(LibFileName)]
        public static extern void GRControlsChangeItemOrder(IntPtr Handle, int Index, int NewOrder);
        [DllImport(LibFileName)]
        public static extern int GRControlsIndexByName(IntPtr Handle, string Name);
        [DllImport(LibFileName)]
        public static extern IntPtr GRControlsItemAt(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRControlsRemove(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRControlsRemoveAll(IntPtr Handle);

        //Fields原始API
        [DllImport(LibFileName)]
        public static extern int GRFieldsGetCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFieldsAdd(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRFieldsChangeItemOrder(IntPtr Handle, int Index, int NewOrder);
        [DllImport(LibFileName)]
        public static extern int GRFieldsIndexByName(IntPtr Handle, string Name);
        [DllImport(LibFileName)]
        public static extern IntPtr GRFieldsItemAt(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRFieldsRemove(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRFieldsRemoveAll(IntPtr Handle);

        //ColumnContentCells原始API
        [DllImport(LibFileName)]
        public static extern int GRColumnContentCellsGetCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRColumnContentCellsIndexByName(IntPtr Handle, string Name);
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnContentCellsItemAt(IntPtr Handle, int Index);

        //ColumnTitleCells原始API
        [DllImport(LibFileName)]
        public static extern int GRColumnTitleCellsGetCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnTitleCellsAddGroup(IntPtr Handle, string Name, string Title);
        [DllImport(LibFileName)]
        public static extern void GRColumnTitleCellsChangeItemOrder(IntPtr Handle, int Index, int NewOrder);
        [DllImport(LibFileName)]
        public static extern int GRColumnTitleCellsIndexByName(IntPtr Handle, string Name);
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnTitleCellsItemAt(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRColumnTitleCellsRemoveGroup(IntPtr Handle, int Index);

        //Columns原始API
        [DllImport(LibFileName)]
        public static extern int GRColumnsGetCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnsAdd(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRColumnsChangeItemOrder(IntPtr Handle, int Index, int NewOrder);
        [DllImport(LibFileName)]
        public static extern int GRColumnsIndexByName(IntPtr Handle, string Name);
        [DllImport(LibFileName)]
        public static extern IntPtr GRColumnsItemAt(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRColumnsRemove(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRColumnsRemoveAll(IntPtr Handle);

        //Groups原始API
        [DllImport(LibFileName)]
        public static extern int GRGroupsGetCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRGroupsAdd(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRGroupsChangeItemOrder(IntPtr Handle, int Index, int NewOrder);
        [DllImport(LibFileName)]
        public static extern int GRGroupsIndexByName(IntPtr Handle, string Name);
        [DllImport(LibFileName)]
        public static extern IntPtr GRGroupsItemAt(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRGroupsRemove(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRGroupsRemoveAll(IntPtr Handle);

        //Parameters原始API
        [DllImport(LibFileName)]
        public static extern int GRParametersGetCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRParametersAdd(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRParametersChangeItemOrder(IntPtr Handle, int Index, int NewOrder);
        [DllImport(LibFileName)]
        public static extern int GRParametersIndexByName(IntPtr Handle, string Name);
        [DllImport(LibFileName)]
        public static extern IntPtr GRParametersItemAt(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRParametersRemove(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRParametersRemoveAll(IntPtr Handle);

        //ChartSeriess原始API
        [DllImport(LibFileName)]
        public static extern int GRChartSeriessGetCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartSeriessAdd(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriessChangeItemOrder(IntPtr Handle, int Index, int NewOrder);
        [DllImport(LibFileName)]
        public static extern int GRChartSeriessIndexByName(IntPtr Handle, string Name);
        [DllImport(LibFileName)]
        public static extern IntPtr GRChartSeriessItemAt(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriessRemove(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRChartSeriessRemoveAll(IntPtr Handle);

        //ReportFooters原始API
        [DllImport(LibFileName)]
        public static extern int GRReportFootersGetCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportFootersAdd(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRReportFootersChangeItemOrder(IntPtr Handle, int Index, int NewOrder);
        [DllImport(LibFileName)]
        public static extern int GRReportFootersIndexByName(IntPtr Handle, string Name);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportFootersItemAt(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRReportFootersRemove(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRReportFootersRemoveAll(IntPtr Handle);

        //ReportHeaders原始API
        [DllImport(LibFileName)]
        public static extern int GRReportHeadersGetCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportHeadersAdd(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRReportHeadersChangeItemOrder(IntPtr Handle, int Index, int NewOrder);
        [DllImport(LibFileName)]
        public static extern int GRReportHeadersIndexByName(IntPtr Handle, string Name);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportHeadersItemAt(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRReportHeadersRemove(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRReportHeadersRemoveAll(IntPtr Handle);

        //ExportOption原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRExportOptionGetAsE2CellOption(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRExportOptionGetAsE2CSVOption(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRExportOptionGetAsE2HTMOption(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRExportOptionGetAsE2IMGOption(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRExportOptionGetAsE2PDFOption(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRExportOptionGetAsE2RTFOption(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRExportOptionGetAsE2TXTOption(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRExportOptionGetAsE2XLSOption(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern ExportType GRExportOptionGetExportType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRExportOptionGetFileName(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRExportOptionSetFileName(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRExportOptionGetMailSubject(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRExportOptionSetMailSubject(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRExportOptionGetMailText(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRExportOptionSetMailText(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRExportOptionGetMailTo(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRExportOptionSetMailTo(IntPtr handle, string val);

        //E2CellOption原始API
        [DllImport(LibFileName)]
        public static extern bool GRE2CellOptionGetColumnAsDetailGrid(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2CellOptionSetColumnAsDetailGrid(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRE2CellOptionGetColumnTitleForbidRepeat(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2CellOptionSetColumnTitleForbidRepeat(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRE2CellOptionGetExportPageBreak(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2CellOptionSetExportPageBreak(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRE2CellOptionGetExportPageHeaderFooter(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2CellOptionSetExportPageHeaderFooter(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRE2CellOptionGetOnlyExportDetailGrid(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2CellOptionSetOnlyExportDetailGrid(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRE2CellOptionGetOnlyExportPureText(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2CellOptionSetOnlyExportPureText(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRE2CellOptionGetSameAsPrint(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2CellOptionSetSameAsPrint(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRE2CellOptionGetSupressEmptyLines(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2CellOptionSetSupressEmptyLines(IntPtr handle, bool val);

        //E2XLSOption原始API
        [DllImport(LibFileName)]
        public static extern bool GRE2XLSOptionGetExpandRowHeight(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2XLSOptionSetExpandRowHeight(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern int GRE2XLSOptionGetMinColumnWidth(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2XLSOptionSetMinColumnWidth(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern int GRE2XLSOptionGetMinRowHeight(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2XLSOptionSetMinRowHeight(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern int GRE2XLSOptionGetNewSheetRows(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2XLSOptionSetNewSheetRows(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern double GRE2XLSOptionGetPageBottomMargin(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2XLSOptionSetPageBottomMargin(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRE2XLSOptionGetPageLeftMargin(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2XLSOptionSetPageLeftMargin(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRE2XLSOptionGetPageRightMargin(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2XLSOptionSetPageRightMargin(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRE2XLSOptionGetPageTopMargin(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2XLSOptionSetPageTopMargin(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern bool GRE2XLSOptionGetToXlsxFormat(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2XLSOptionSetToXlsxFormat(IntPtr handle, bool val);

        //E2HTMOption原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRE2HTMOptionGetPicturePath(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2HTMOptionSetPicturePath(IntPtr handle, string val);

        //E2TXTOption原始API
        [DllImport(LibFileName)]
        public static extern TextEncodeMode GRE2TXTOptionGetTextEncode(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2TXTOptionSetTextEncode(IntPtr handle, TextEncodeMode val);
        [DllImport(LibFileName)]
        public static extern bool GRE2TXTOptionGetUsingTabChar(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2TXTOptionSetUsingTabChar(IntPtr handle, bool val);

        //E2CSVOption原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRE2CSVOptionGetDelimiterChar(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2CSVOptionSetDelimiterChar(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRE2CSVOptionGetQuoteText(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2CSVOptionSetQuoteText(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern TextEncodeMode GRE2CSVOptionGetTextEncode(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2CSVOptionSetTextEncode(IntPtr handle, TextEncodeMode val);

        //E2RTFOption原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRE2RTFOptionGetAuthor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2RTFOptionSetAuthor(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRE2RTFOptionGetCreator(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2RTFOptionSetCreator(IntPtr handle, string val);

        //E2PDFOption原始API
        [DllImport(LibFileName)]
        public static extern IntPtr GRE2PDFOptionGetAuthor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2PDFOptionSetAuthor(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRE2PDFOptionGetCompressed(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2PDFOptionSetCompressed(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRE2PDFOptionGetCreator(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2PDFOptionSetCreator(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRE2PDFOptionGetFontEmbedding(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2PDFOptionSetFontEmbedding(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRE2PDFOptionGetKeywords(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2PDFOptionSetKeywords(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRE2PDFOptionGetOwnerPassword(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2PDFOptionSetOwnerPassword(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRE2PDFOptionGetProducer(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2PDFOptionSetProducer(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRE2PDFOptionGetSubject(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2PDFOptionSetSubject(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRE2PDFOptionGetUserPassword(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2PDFOptionSetUserPassword(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRE2PDFOptionGetDocPermission(IntPtr Handle, PDFDocPermission Permission);
        [DllImport(LibFileName)]
        public static extern void GRE2PDFOptionSetDocPermission(IntPtr Handle, PDFDocPermission Permission, bool Enabled);

        //E2IMGOption原始API
        [DllImport(LibFileName)]
        public static extern bool GRE2IMGOptionGetAllInOne(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2IMGOptionSetAllInOne(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern int GRE2IMGOptionGetDPI(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2IMGOptionSetDPI(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern bool GRE2IMGOptionGetDrawPageBox(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2IMGOptionSetDrawPageBox(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern ExportImageType GRE2IMGOptionGetImageType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2IMGOptionSetImageType(IntPtr handle, ExportImageType val);
        [DllImport(LibFileName)]
        public static extern int GRE2IMGOptionGetVertGap(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRE2IMGOptionSetVertGap(IntPtr handle, int val);

        //Printer原始API
        [DllImport(LibFileName)]
        public static extern double GRPrinterGetBottomMargin(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetBottomMargin(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern bool GRPrinterGetCanCollate(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRPrinterGetCanDuplex(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRPrinterGetCollate(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetCollate(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern int GRPrinterGetCopies(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetCopies(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern int GRPrinterGetCurPageNo(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetCurPageNo(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern double GRPrinterGetDesignBottomMargin(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetDesignBottomMargin(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRPrinterGetDesignLeftMargin(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetDesignLeftMargin(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRPrinterGetDesignPaperLength(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetDesignPaperLength(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRPrinterGetDesignPaperName(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetDesignPaperName(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern PaperOrientation GRPrinterGetDesignPaperOrientation(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetDesignPaperOrientation(IntPtr handle, PaperOrientation val);
        [DllImport(LibFileName)]
        public static extern int GRPrinterGetDesignPaperSize(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetDesignPaperSize(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern double GRPrinterGetDesignPaperWidth(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetDesignPaperWidth(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRPrinterGetDesignPrinterName(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetDesignPrinterName(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern double GRPrinterGetDesignRightMargin(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetDesignRightMargin(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRPrinterGetDesignTopMargin(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetDesignTopMargin(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern DuplexKind GRPrinterGetDuplex(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetDuplex(IntPtr handle, DuplexKind val);
        [DllImport(LibFileName)]
        public static extern double GRPrinterGetLeftMargin(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetLeftMargin(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern bool GRPrinterGetOnline(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRPrinterGetPageCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetPageCount(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern double GRPrinterGetPaperLength(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetPaperLength(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRPrinterGetPaperName(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetPaperName(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern PaperOrientation GRPrinterGetPaperOrientation(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetPaperOrientation(IntPtr handle, PaperOrientation val);
        [DllImport(LibFileName)]
        public static extern int GRPrinterGetPaperSize(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetPaperSize(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern PaperSourceKind GRPrinterGetPaperSource(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetPaperSource(IntPtr handle, PaperSourceKind val);
        [DllImport(LibFileName)]
        public static extern double GRPrinterGetPaperWidth(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetPaperWidth(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRPrinterGetPrinterName(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetPrinterName(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRPrinterGetPrintOffsetSaveToLocal(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetPrintOffsetSaveToLocal(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern double GRPrinterGetPrintOffsetX(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetPrintOffsetX(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern double GRPrinterGetPrintOffsetY(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetPrintOffsetY(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern PrintPageType GRPrinterGetPrintPageType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetPrintPageType(IntPtr handle, PrintPageType val);
        [DllImport(LibFileName)]
        public static extern PrintRangeType GRPrinterGetPrintRangeType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetPrintRangeType(IntPtr handle, PrintRangeType val);
        [DllImport(LibFileName)]
        public static extern DrawRotation GRPrinterGetPrintRotation(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetPrintRotation(IntPtr handle, DrawRotation val);
        [DllImport(LibFileName)]
        public static extern double GRPrinterGetRightMargin(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetRightMargin(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRPrinterGetSelectionPrintPages(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetSelectionPrintPages(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern SheetPages GRPrinterGetSheetPages(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetSheetPages(IntPtr handle, SheetPages val);
        [DllImport(LibFileName)]
        public static extern int GRPrinterGetSheetPaperSize(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetSheetPaperSize(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern bool GRPrinterGetSupportCustomPaper(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern double GRPrinterGetTopMargin(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetTopMargin(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern void GRPrinterSetCustomPaperSize(IntPtr Handle, double Width, double Length);

        //ImageList原始API
        [DllImport(LibFileName)]
        public static extern int GRImageListGetCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRImageListAddFromBinary(IntPtr Handle, IntPtr BinaryObject);
        [DllImport(LibFileName)]
        public static extern int GRImageListAddFromFile(IntPtr Handle, string PathFile);
        [DllImport(LibFileName)]
        public static extern IntPtr GRImageListItem(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRImageListRemove(IntPtr Handle, int Index);
        [DllImport(LibFileName)]
        public static extern void GRImageListRemoveAll(IntPtr Handle);

        //Report原始API
        [DllImport(LibFileName)]
        public static extern bool GRReportGetAbortOnError(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetAbortOnError(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRReportGetAlignToGrid(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetAlignToGrid(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetAuthor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetAuthor(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern uint GRReportGetBackColor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetBackColor(IntPtr handle, uint val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetBackImage(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetBackImage(IntPtr handle, IntPtr val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetBackImageFile(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetBackImageFile(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRReportGetBackImagePreview(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetBackImagePreview(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRReportGetBackImagePrint(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetBackImagePrint(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetBeforeSortScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetBeforeSortScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern int GRReportGetCodePage(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetCodePage(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetConnectionString(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetConnectionString(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRReportGetContinuePrint(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetContinuePrint(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRReportGetDataLoaded(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetDescription(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetDescription(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetDetailGrid(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern ReportDisplayMode GRReportGetDisplayMode(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern DocType GRReportGetDocType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetDocType(IntPtr handle, DocType val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetExportBeginScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetExportBeginScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetFiringReport(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRReportGetFirstPass(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetFloatControls(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetFont(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetGeneratePagesBeginScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetGeneratePagesBeginScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetGeneratePagesEndScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetGeneratePagesEndScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetGlobalScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetGlobalScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetGraphics(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRReportGetGridColsPerUnit(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetGridColsPerUnit(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern int GRReportGetGridRowsPerUnit(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetGridRowsPerUnit(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern bool GRReportGetHasFloatControl(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetImageList(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetInitializeScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetInitializeScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRReportGetIsBlank(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRReportGetLanguage(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetLanguage(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetLatestLoadedFile(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetLatestLoadedFile(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRReportGetMirrorMargins(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetMirrorMargins(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern bool GRReportGetMonoPrint(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetMonoPrint(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern double GRReportGetPageBlankHeight(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern int GRReportGetPageDivideCount(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetPageDivideCount(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern bool GRReportGetPageDivideLine(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetPageDivideLine(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern double GRReportGetPageDivideSpacing(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetPageDivideSpacing(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetPageEndScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetPageEndScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetPageFooter(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetPageHeader(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetPageStartScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetPageStartScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetParameters(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetParentReport(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRReportGetPrintAsDesignPaper(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetPrintAsDesignPaper(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetPrinter(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetProcessBeginScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetProcessBeginScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetProcessEndScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetProcessEndScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetQuerySQL(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetQuerySQL(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetReportFooters(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetReportHeaders(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern bool GRReportGetRunning(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetRunningDetailGrid(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern ScriptType GRReportGetScriptType(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetScriptType(IntPtr handle, ScriptType val);
        [DllImport(LibFileName)]
        public static extern bool GRReportGetShowGrid(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetShowGrid(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern int GRReportGetShowMoneyDigit(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetShowMoneyDigit(IntPtr handle, int val);
        [DllImport(LibFileName)]
        public static extern uint GRReportGetShowMoneyLineColor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetShowMoneyLineColor(IntPtr handle, uint val);
        [DllImport(LibFileName)]
        public static extern bool GRReportGetShowMoneySepLineColor(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetShowMoneySepLineColor(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern double GRReportGetShowMoneyWidth(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetShowMoneyWidth(IntPtr handle, double val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetShowPreviewWndScript(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetShowPreviewWndScript(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern bool GRReportGetSkipQuery(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetSkipQuery(IntPtr handle, bool val);
        [DllImport(LibFileName)]
        public static extern StorageFormat GRReportGetStorageFormat(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetStorageFormat(IntPtr handle, StorageFormat val);
        [DllImport(LibFileName)]
        public static extern TextEncodeMode GRReportGetTextEncode(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetTextEncode(IntPtr handle, TextEncodeMode val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetTitle(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetTitle(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern Unit GRReportGetUnit(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetUnit(IntPtr handle, Unit val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetUtility(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetVersion(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetWatermark(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetWatermark(IntPtr handle, IntPtr val);
        [DllImport(LibFileName)]
        public static extern PictureAlignment GRReportGetWatermarkAlignment(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetWatermarkAlignment(IntPtr handle, PictureAlignment val);
        [DllImport(LibFileName)]
        public static extern PictureSizeMode GRReportGetWatermarkSizeMode(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetWatermarkSizeMode(IntPtr handle, PictureSizeMode val);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGetXmlTableName(IntPtr handle);
        [DllImport(LibFileName)]
        public static extern void GRReportSetXmlTableName(IntPtr handle, string val);
        [DllImport(LibFileName)]
        public static extern void GRReportAbort(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRReportAbortExport(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRReportAbortPrint(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportAddParameter(IntPtr Handle, string Name, ParameterDataType Type);
        [DllImport(LibFileName)]
        public static extern bool GRReportBeginLoopPrint(IntPtr Handle, PrintGenerateStyle GenerateStyle, bool ShowPrintDlg);
        [DllImport(LibFileName)]
        public static extern void GRReportCancelLoadData(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRReportClear(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportColumnByName(IntPtr Handle, string Name);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportControlByName(IntPtr Handle, string Name);
        [DllImport(LibFileName)]
        public static extern void GRReportDeleteDetailGrid(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRReportDeletePageFooter(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRReportDeletePageHeader(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRReportEndLoopPrint(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern bool GRReportExport(IntPtr Handle, bool DoneOpen);
        [DllImport(LibFileName)]
        public static extern bool GRReportExportDirect(IntPtr Handle, ExportType ExportType, string FileName, bool ShowOptionDlg, bool DoneOpen);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportExportDirectToBinaryObject(IntPtr Handle, ExportType ExportType);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportExportToBinaryObject(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportExtractXMLFromURL(IntPtr Handle, string URL);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportFieldByDBName(IntPtr Handle, string DBFieldName);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportFieldByName(IntPtr Handle, string Name);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportFindFirstControl(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportFindNextControl(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRReportForceNewPage(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportGenerateDocumentData(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRReportGenerateDocumentFile(IntPtr Handle, string PathFile);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportInsertDetailGrid(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportInsertPageFooter(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportInsertPageHeader(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRReportLoadBackImageFromFile(IntPtr Handle, string PathFile);
        [DllImport(LibFileName)]
        public static extern bool GRReportLoadDataFromURL(IntPtr Handle, string URL);
        [DllImport(LibFileName)]
        public static extern bool GRReportLoadDataFromXML(IntPtr Handle, string DataText);
        [DllImport(LibFileName)]
        public static extern bool GRReportLoadFromBinary(IntPtr Handle, IntPtr BinaryObject);
        [DllImport(LibFileName)]
        public static extern bool GRReportLoadFromFile(IntPtr Handle, string FileName);
        [DllImport(LibFileName)]
        public static extern bool GRReportLoadFromMemory(IntPtr Handle, IntPtr pData, int ByteCount);
        [DllImport(LibFileName)]
        public static extern bool GRReportLoadFromStr(IntPtr Handle, string Data);
        [DllImport(LibFileName)]
        public static extern bool GRReportLoadFromURL(IntPtr Handle, string URL);
        [DllImport(LibFileName)]
        public static extern void GRReportLoadWatermarkFromFile(IntPtr Handle, string PathFile);
        [DllImport(LibFileName)]
        public static extern void GRReportLoopPrint(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportParameterByName(IntPtr Handle, string Name);
        [DllImport(LibFileName)]
        public static extern double GRReportPixelsToUnit(IntPtr Handle, int Pixels);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportPrepareExport(IntPtr Handle, ExportType ExportType);
        [DllImport(LibFileName)]
        public static extern bool GRReportPrepareLoadData(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern void GRReportPrint(IntPtr Handle, bool ShowPrintDialog);
        [DllImport(LibFileName)]
        public static extern bool GRReportPrintDocumentData(IntPtr Handle, IntPtr DocumentData);
        [DllImport(LibFileName)]
        public static extern void GRReportPrintEx(IntPtr Handle, PrintGenerateStyle GenerateStyle, bool ShowPrintDialog);
        [DllImport(LibFileName)]
        public static extern bool GRReportSaveToBinary(IntPtr Handle, IntPtr BinaryObject);
        [DllImport(LibFileName)]
        public static extern bool GRReportSaveToFile(IntPtr Handle, string FileName);
        [DllImport(LibFileName)]
        public static extern IntPtr GRReportSaveToStr(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern double GRReportSystemVarValue(IntPtr Handle, SystemVarType SystemVar);
        [DllImport(LibFileName)]
        public static extern double GRReportSystemVarValue2(IntPtr Handle, SystemVarType SystemVar, int GroupIndex);
        [DllImport(LibFileName)]
        public static extern double GRReportUnitToPixels(IntPtr Handle, double UnitValue);
        [DllImport(LibFileName)]
        public static extern void GRReportUnprepareExport(IntPtr Handle);
        [DllImport(LibFileName)]
        public static extern int GRReportWebRegisterStatus(IntPtr Handle);
    }
    public class Base
    {
        protected IntPtr handle = IntPtr.Zero;

        public Base(IntPtr _handle)
        {
            handle = _handle;
        }

        public IntPtr NativeHandle
        {
            get
            {
                return handle;
            }
        }

        public bool Active
        {
            get
            {
                return handle != IntPtr.Zero;
            }
        }

        protected void _Free()
        {
            handle = IntPtr.Zero;
        }
    }

    public class Font : Base
    {
        public Font(IntPtr _handle)
            : base(_handle)
        {
        }
        public bool Bold
        {
            get
            {
                return RawAPI.GRFontGetBold(handle);
            }
            set
            {
                RawAPI.GRFontSetBold(handle, value);
            }
        }
        public int Charset
        {
            get
            {
                return RawAPI.GRFontGetCharset(handle);
            }
            set
            {
                RawAPI.GRFontSetCharset(handle, value);
            }
        }
        public bool Italic
        {
            get
            {
                return RawAPI.GRFontGetItalic(handle);
            }
            set
            {
                RawAPI.GRFontSetItalic(handle, value);
            }
        }
        public string Name
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRFontGetName(handle));
            }
            set
            {
                RawAPI.GRFontSetName(handle, value);
            }
        }
        public bool ParentFont
        {
            get
            {
                return RawAPI.GRFontGetParentFont(handle);
            }
        }
        public double Point
        {
            get
            {
                return RawAPI.GRFontGetPoint(handle);
            }
            set
            {
                RawAPI.GRFontSetPoint(handle, value);
            }
        }
        public bool Strikethrough
        {
            get
            {
                return RawAPI.GRFontGetStrikethrough(handle);
            }
            set
            {
                RawAPI.GRFontSetStrikethrough(handle, value);
            }
        }
        public bool Underline
        {
            get
            {
                return RawAPI.GRFontGetUnderline(handle);
            }
            set
            {
                RawAPI.GRFontSetUnderline(handle, value);
            }
        }
        public int Weight
        {
            get
            {
                return RawAPI.GRFontGetWeight(handle);
            }
            set
            {
                RawAPI.GRFontSetWeight(handle, value);
            }
        }

        public Font Clone()
        {
            IntPtr theHandle = RawAPI.GRFontClone(handle);
            return theHandle != IntPtr.Zero ? new Font(theHandle) : null;
        }
        public void Free()
        {
            RawAPI.GRFontFree(handle);
            _Free();
        }
        public void ToDefault()
        {
            RawAPI.GRFontToDefault(handle);
        }
    }

    public class Pen : Base
    {
        public Pen(IntPtr _handle)
            : base(_handle)
        {
        }
        public uint Color
        {
            get
            {
                return RawAPI.GRPenGetColor(handle);
            }
            set
            {
                RawAPI.GRPenSetColor(handle, value);
            }
        }
        public PenStyle Style
        {
            get
            {
                return RawAPI.GRPenGetStyle(handle);
            }
            set
            {
                RawAPI.GRPenSetStyle(handle, value);
            }
        }
        public double Width
        {
            get
            {
                return RawAPI.GRPenGetWidth(handle);
            }
            set
            {
                RawAPI.GRPenSetWidth(handle, value);
            }
        }

    }

    public class Border : Base
    {
        public Border(IntPtr _handle)
            : base(_handle)
        {
        }
        public int InnerIndent
        {
            get
            {
                return RawAPI.GRBorderGetInnerIndent(handle);
            }
            set
            {
                RawAPI.GRBorderSetInnerIndent(handle, value);
            }
        }
        public Pen InnerPen
        {
            get
            {
                IntPtr theHandle = RawAPI.GRBorderGetInnerPen(handle);
                return theHandle != IntPtr.Zero ? new Pen(theHandle) : null;
            }
        }
        public int InnerStyles
        {
            get
            {
                return RawAPI.GRBorderGetInnerStyles(handle);
            }
            set
            {
                RawAPI.GRBorderSetInnerStyles(handle, value);
            }
        }
        public Pen Pen
        {
            get
            {
                IntPtr theHandle = RawAPI.GRBorderGetPen(handle);
                return theHandle != IntPtr.Zero ? new Pen(theHandle) : null;
            }
        }
        public bool Shadow
        {
            get
            {
                return RawAPI.GRBorderGetShadow(handle);
            }
            set
            {
                RawAPI.GRBorderSetShadow(handle, value);
            }
        }
        public uint ShadowColor
        {
            get
            {
                return RawAPI.GRBorderGetShadowColor(handle);
            }
            set
            {
                RawAPI.GRBorderSetShadowColor(handle, value);
            }
        }
        public int ShadowWidth
        {
            get
            {
                return RawAPI.GRBorderGetShadowWidth(handle);
            }
            set
            {
                RawAPI.GRBorderSetShadowWidth(handle, value);
            }
        }
        public int Styles
        {
            get
            {
                return RawAPI.GRBorderGetStyles(handle);
            }
            set
            {
                RawAPI.GRBorderSetStyles(handle, value);
            }
        }

        public bool GetInnerStyle(BorderStyles BorderSide)
        {
            return RawAPI.GRBorderGetInnerStyle(handle, BorderSide);
        }
        public bool GetStyle(BorderStyles BorderSide)
        {
            return RawAPI.GRBorderGetStyle(handle, BorderSide);
        }
        public void SetInnerStyle(BorderStyles BorderSide, bool ToShow)
        {
            RawAPI.GRBorderSetInnerStyle(handle, BorderSide, ToShow);
        }
        public void SetStyle(BorderStyles BorderSide, bool ToShow)
        {
            RawAPI.GRBorderSetStyle(handle, BorderSide, ToShow);
        }
    }

    public class TextFormat : Base
    {
        public TextFormat(IntPtr _handle)
            : base(_handle)
        {
        }
        public double CharacterSpacing
        {
            get
            {
                return RawAPI.GRTextFormatGetCharacterSpacing(handle);
            }
            set
            {
                RawAPI.GRTextFormatSetCharacterSpacing(handle, value);
            }
        }
        public bool EndEllipsis
        {
            get
            {
                return RawAPI.GRTextFormatGetEndEllipsis(handle);
            }
            set
            {
                RawAPI.GRTextFormatSetEndEllipsis(handle, value);
            }
        }
        public double FirstCharIndent
        {
            get
            {
                return RawAPI.GRTextFormatGetFirstCharIndent(handle);
            }
            set
            {
                RawAPI.GRTextFormatSetFirstCharIndent(handle, value);
            }
        }
        public double FontWidthRatio
        {
            get
            {
                return RawAPI.GRTextFormatGetFontWidthRatio(handle);
            }
            set
            {
                RawAPI.GRTextFormatSetFontWidthRatio(handle, value);
            }
        }
        public bool HtmlTags
        {
            get
            {
                return RawAPI.GRTextFormatGetHtmlTags(handle);
            }
            set
            {
                RawAPI.GRTextFormatSetHtmlTags(handle, value);
            }
        }
        public double LineSpacing
        {
            get
            {
                return RawAPI.GRTextFormatGetLineSpacing(handle);
            }
            set
            {
                RawAPI.GRTextFormatSetLineSpacing(handle, value);
            }
        }
        public double ParagraphSpacing
        {
            get
            {
                return RawAPI.GRTextFormatGetParagraphSpacing(handle);
            }
            set
            {
                RawAPI.GRTextFormatSetParagraphSpacing(handle, value);
            }
        }
        public TextAlign TextAlign
        {
            get
            {
                return RawAPI.GRTextFormatGetTextAlign(handle);
            }
            set
            {
                RawAPI.GRTextFormatSetTextAlign(handle, value);
            }
        }
        public double TextAngle
        {
            get
            {
                return RawAPI.GRTextFormatGetTextAngle(handle);
            }
            set
            {
                RawAPI.GRTextFormatSetTextAngle(handle, value);
            }
        }
        public TextOrientation TextOrientation
        {
            get
            {
                return RawAPI.GRTextFormatGetTextOrientation(handle);
            }
            set
            {
                RawAPI.GRTextFormatSetTextOrientation(handle, value);
            }
        }
        public bool WordWrap
        {
            get
            {
                return RawAPI.GRTextFormatGetWordWrap(handle);
            }
            set
            {
                RawAPI.GRTextFormatSetWordWrap(handle, value);
            }
        }

        public TextFormat Clone()
        {
            IntPtr theHandle = RawAPI.GRTextFormatClone(handle);
            return theHandle != IntPtr.Zero ? new TextFormat(theHandle) : null;
        }
        public void Free()
        {
            RawAPI.GRTextFormatFree(handle);
            _Free();
        }
    }

    public class Picture : Base
    {
        public Picture(IntPtr _handle)
            : base(_handle)
        {
        }
        public int Height
        {
            get
            {
                return RawAPI.GRPictureGetHeight(handle);
            }
        }
        public PictureType Type
        {
            get
            {
                return RawAPI.GRPictureGetType(handle);
            }
        }
        public int Width
        {
            get
            {
                return RawAPI.GRPictureGetWidth(handle);
            }
        }

        public void Free()
        {
            RawAPI.GRPictureFree(handle);
            _Free();
        }
        public bool LoadFromBinary(BinaryObject BinaryObject)
        {
            return RawAPI.GRPictureLoadFromBinary(handle, BinaryObject.NativeHandle);
        }
        public bool LoadFromFile(string PathFile)
        {
            return RawAPI.GRPictureLoadFromFile(handle, PathFile);
        }
        public void LoadFromMemory(IntPtr Data, int DataLen)
        {
            RawAPI.GRPictureLoadFromMemory(handle, Data, DataLen);
        }
        public bool SaveToFile(string PathFile)
        {
            return RawAPI.GRPictureSaveToFile(handle, PathFile);
        }
    }

    public class BinaryObject : Base
    {
        public BinaryObject(IntPtr _handle)
            : base(_handle)
        {
        }
        public string AsBase64Text
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRBinaryObjectGetAsBase64Text(handle));
            }
        }
        public IntPtr DataBuf
        {
            get
            {
                return RawAPI.GRBinaryObjectGetDataBuf(handle);
            }
        }
        public int DataSize
        {
            get
            {
                return RawAPI.GRBinaryObjectGetDataSize(handle);
            }
        }

        public void Free()
        {
            RawAPI.GRBinaryObjectFree(handle);
            _Free();
        }
        public bool LoadFromBase64Text(string Base64Text)
        {
            return RawAPI.GRBinaryObjectLoadFromBase64Text(handle, Base64Text);
        }
        public bool LoadFromField(Field Field)
        {
            return RawAPI.GRBinaryObjectLoadFromField(handle, Field.NativeHandle);
        }
        public bool LoadFromFile(string PathFile)
        {
            return RawAPI.GRBinaryObjectLoadFromFile(handle, PathFile);
        }
        public bool LoadFromMemory(IntPtr pData, int ByteCount)
        {
            return RawAPI.GRBinaryObjectLoadFromMemory(handle, pData, ByteCount);
        }
        public bool SaveToFile(string PathFile)
        {
            return RawAPI.GRBinaryObjectSaveToFile(handle, PathFile);
        }
    }

    public class DateTime : Base
    {
        public DateTime(IntPtr _handle)
            : base(_handle)
        {
        }
        public double AsFloat
        {
            get
            {
                return RawAPI.GRDateTimeGetAsFloat(handle);
            }
            set
            {
                RawAPI.GRDateTimeSetAsFloat(handle, value);
            }
        }
        public int Day
        {
            get
            {
                return RawAPI.GRDateTimeGetDay(handle);
            }
        }
        public int DayOfWeek
        {
            get
            {
                return RawAPI.GRDateTimeGetDayOfWeek(handle);
            }
        }
        public int DayOfYear
        {
            get
            {
                return RawAPI.GRDateTimeGetDayOfYear(handle);
            }
        }
        public int Hour
        {
            get
            {
                return RawAPI.GRDateTimeGetHour(handle);
            }
        }
        public int Minute
        {
            get
            {
                return RawAPI.GRDateTimeGetMinute(handle);
            }
        }
        public int Month
        {
            get
            {
                return RawAPI.GRDateTimeGetMonth(handle);
            }
        }
        public int Second
        {
            get
            {
                return RawAPI.GRDateTimeGetSecond(handle);
            }
        }
        public long Value
        {
            get
            {
                return RawAPI.GRDateTimeGetValue(handle);
            }
            set
            {
                RawAPI.GRDateTimeSetValue(handle, value);
            }
        }
        public int Year
        {
            get
            {
                return RawAPI.GRDateTimeGetYear(handle);
            }
        }

        public string Format(string Format)
        {
            return Marshal.PtrToStringUni(RawAPI.GRDateTimeFormat(handle, Format));
        }
        public void Free()
        {
            RawAPI.GRDateTimeFree(handle);
            _Free();
        }
        public void ValueFromDate(int Year, int Month, int Day)
        {
            RawAPI.GRDateTimeValueFromDate(handle, Year, Month, Day);
        }
        public void ValueFromDateTime(int Year, int Month, int Day, int Hour, int Minute, int Second)
        {
            RawAPI.GRDateTimeValueFromDateTime(handle, Year, Month, Day, Hour, Minute, Second);
        }
    }

    public class Utility : Base
    {
        public Utility(IntPtr _handle)
            : base(_handle)
        {
        }
        public double CCPInnerX
        {
            get
            {
                return RawAPI.GRUtilityGetCCPInnerX(handle);
            }
        }
        public double CCPInnerY
        {
            get
            {
                return RawAPI.GRUtilityGetCCPInnerY(handle);
            }
        }
        public double CCPOuterX
        {
            get
            {
                return RawAPI.GRUtilityGetCCPOuterX(handle);
            }
        }
        public double CCPOuterY
        {
            get
            {
                return RawAPI.GRUtilityGetCCPOuterY(handle);
            }
        }
        public bool DisableExceptionMsg
        {
            get
            {
                return RawAPI.GRUtilityGetDisableExceptionMsg(handle);
            }
            set
            {
                RawAPI.GRUtilitySetDisableExceptionMsg(handle, value);
            }
        }
        public bool DisableHttpFailedMsg
        {
            get
            {
                return RawAPI.GRUtilityGetDisableHttpFailedMsg(handle);
            }
            set
            {
                RawAPI.GRUtilitySetDisableHttpFailedMsg(handle, value);
            }
        }
        public bool DisableNormalMsg
        {
            get
            {
                return RawAPI.GRUtilityGetDisableNormalMsg(handle);
            }
            set
            {
                RawAPI.GRUtilitySetDisableNormalMsg(handle, value);
            }
        }
        public double ShrinkFontMinWidthRatio
        {
            get
            {
                return RawAPI.GRUtilityGetShrinkFontMinWidthRatio(handle);
            }
            set
            {
                RawAPI.GRUtilitySetShrinkFontMinWidthRatio(handle, value);
            }
        }
        public string TextLeftPunctuations
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRUtilityGetTextLeftPunctuations(handle));
            }
            set
            {
                RawAPI.GRUtilitySetTextLeftPunctuations(handle, value);
            }
        }
        public string TextRightPunctuations
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRUtilityGetTextRightPunctuations(handle));
            }
            set
            {
                RawAPI.GRUtilitySetTextRightPunctuations(handle, value);
            }
        }
        public bool TextWrapByWord
        {
            get
            {
                return RawAPI.GRUtilityGetTextWrapByWord(handle);
            }
            set
            {
                RawAPI.GRUtilitySetTextWrapByWord(handle, value);
            }
        }
        public bool TextWrapToJustify
        {
            get
            {
                return RawAPI.GRUtilityGetTextWrapToJustify(handle);
            }
            set
            {
                RawAPI.GRUtilitySetTextWrapToJustify(handle, value);
            }
        }

        public void Alert(string Msg)
        {
            RawAPI.GRUtilityAlert(handle, Msg);
        }
        public void CalcCurveControlPoints(double xFirst, double yFirst, double xMiddle, double yMiddle, double xAfter, double yAfter)
        {
            RawAPI.GRUtilityCalcCurveControlPoints(handle, xFirst, yFirst, xMiddle, yMiddle, xAfter, yAfter);
        }
        public double CalcDrawFormatTextHeight(string Text, double width, TextFormat TextFormat, Font Font)
        {
            return RawAPI.GRUtilityCalcDrawFormatTextHeight(handle, Text, width, TextFormat.NativeHandle, Font.NativeHandle);
        }
        public int CalcDrawFormatTextOutLen(string Text, double Width, double Height, TextFormat TextFormat, Font Font)
        {
            return RawAPI.GRUtilityCalcDrawFormatTextOutLen(handle, Text, Width, Height, TextFormat.NativeHandle, Font.NativeHandle);
        }
        public double CalcDrawFormatTextWidth(string Text, TextFormat TextFormat, Font Font)
        {
            return RawAPI.GRUtilityCalcDrawFormatTextWidth(handle, Text, TextFormat.NativeHandle, Font.NativeHandle);
        }
        public double CalcLabelTextHeight(string Text, Font Font)
        {
            return RawAPI.GRUtilityCalcLabelTextHeight(handle, Text, Font.NativeHandle);
        }
        public double CalcLabelTextWidth(string Text, Font Font)
        {
            return RawAPI.GRUtilityCalcLabelTextWidth(handle, Text, Font.NativeHandle);
        }
        public int CalcTextOutLen(double Width, double Height, string Text, bool WordWrap, Font Font)
        {
            return RawAPI.GRUtilityCalcTextOutLen(handle, Width, Height, Text, WordWrap, Font.NativeHandle);
        }
        public uint ColorFromRGB(int r, int g, int b)
        {
            return RawAPI.GRUtilityColorFromRGB(handle, r, g, b);
        }
        public BinaryObject CreateBinaryObject()
        {
            IntPtr theHandle = RawAPI.GRUtilityCreateBinaryObject(handle);
            return theHandle != IntPtr.Zero ? new BinaryObject(theHandle) : null;
        }
        public DateTime CreateDateTime()
        {
            IntPtr theHandle = RawAPI.GRUtilityCreateDateTime(handle);
            return theHandle != IntPtr.Zero ? new DateTime(theHandle) : null;
        }
        public bool CreatePathOnNeed(string Path)
        {
            return RawAPI.GRUtilityCreatePathOnNeed(handle, Path);
        }
        public Picture CreatePicture()
        {
            IntPtr theHandle = RawAPI.GRUtilityCreatePicture(handle);
            return theHandle != IntPtr.Zero ? new Picture(theHandle) : null;
        }
        public Recordset CreateRecordset()
        {
            IntPtr theHandle = RawAPI.GRUtilityCreateRecordset(handle);
            return theHandle != IntPtr.Zero ? new Recordset(theHandle) : null;
        }
        public TextFormat CreateTextFormat()
        {
            IntPtr theHandle = RawAPI.GRUtilityCreateTextFormat(handle);
            return theHandle != IntPtr.Zero ? new TextFormat(theHandle) : null;
        }
        public string DateTimeFormat(long Date, string Format)
        {
            return Marshal.PtrToStringUni(RawAPI.GRUtilityDateTimeFormat(handle, Date, Format));
        }
        public void DisableDebug()
        {
            RawAPI.GRUtilityDisableDebug(handle);
        }
        public string ExtractFileName(string PathFile)
        {
            return Marshal.PtrToStringUni(RawAPI.GRUtilityExtractFileName(handle, PathFile));
        }
        public string ExtractFilePath(string PathFile)
        {
            return Marshal.PtrToStringUni(RawAPI.GRUtilityExtractFilePath(handle, PathFile));
        }
        public int GetPrinterCount()
        {
            return RawAPI.GRUtilityGetPrinterCount(handle);
        }
        public string GetPrinterName(int Index)
        {
            return Marshal.PtrToStringUni(RawAPI.GRUtilityGetPrinterName(handle, Index));
        }
        public int GetPrinterPaperCount(string PrinterName)
        {
            return RawAPI.GRUtilityGetPrinterPaperCount(handle, PrinterName);
        }
        public string GetPrinterPaperName(int Index)
        {
            return Marshal.PtrToStringUni(RawAPI.GRUtilityGetPrinterPaperName(handle, Index));
        }
        public string NumberFormat(double Value, string Format)
        {
            return Marshal.PtrToStringUni(RawAPI.GRUtilityNumberFormat(handle, Value, Format));
        }
        public string NumberFormatToBigHZ(double Value, int Decimals)
        {
            return Marshal.PtrToStringUni(RawAPI.GRUtilityNumberFormatToBigHZ(handle, Value, Decimals));
        }
        public string NumberFormatToBigHZAmount(double Value)
        {
            return Marshal.PtrToStringUni(RawAPI.GRUtilityNumberFormatToBigHZAmount(handle, Value));
        }
        public double NumberRound45(double Value, int Decimals)
        {
            return RawAPI.GRUtilityNumberRound45(handle, Value, Decimals);
        }
        public double NumberRound465(double Value, int Decimals)
        {
            return RawAPI.GRUtilityNumberRound465(handle, Value, Decimals);
        }
        public bool PathFileExist(string PathFile)
        {
            return RawAPI.GRUtilityPathFileExist(handle, PathFile);
        }
        public void SetMoneyLabelText(string MoneyLabelText)
        {
            RawAPI.GRUtilitySetMoneyLabelText(handle, MoneyLabelText);
        }
    }

    public class Graphics : Base
    {
        public Graphics(IntPtr _handle)
            : base(_handle)
        {
        }
        public bool ByUnit
        {
            get
            {
                return RawAPI.GRGraphicsGetByUnit(handle);
            }
            set
            {
                RawAPI.GRGraphicsSetByUnit(handle, value);
            }
        }
        public double Height
        {
            get
            {
                return RawAPI.GRGraphicsGetHeight(handle);
            }
        }
        public double Left
        {
            get
            {
                return RawAPI.GRGraphicsGetLeft(handle);
            }
        }
        public double LeftInPaper
        {
            get
            {
                return RawAPI.GRGraphicsGetLeftInPaper(handle);
            }
        }
        public double Top
        {
            get
            {
                return RawAPI.GRGraphicsGetTop(handle);
            }
        }
        public double TopInPaper
        {
            get
            {
                return RawAPI.GRGraphicsGetTopInPaper(handle);
            }
        }
        public double Width
        {
            get
            {
                return RawAPI.GRGraphicsGetWidth(handle);
            }
        }

        public void Arc(double x, double y, double r, double BeginAngleDegree, double EndAngleDegree)
        {
            RawAPI.GRGraphicsArc(handle, x, y, r, BeginAngleDegree, EndAngleDegree);
        }
        public void BeginPath()
        {
            RawAPI.GRGraphicsBeginPath(handle);
        }
        public double CalcDrawFormatTextHeight(string Text, double width, TextFormat TextFormat)
        {
            return RawAPI.GRGraphicsCalcDrawFormatTextHeight(handle, Text, width, TextFormat.NativeHandle);
        }
        public int CalcDrawFormatTextOutLen(string Text, double Width, double Height, TextFormat TextFormat)
        {
            return RawAPI.GRGraphicsCalcDrawFormatTextOutLen(handle, Text, Width, Height, TextFormat.NativeHandle);
        }
        public double CalcDrawFormatTextWidth(string Text, TextFormat TextFormat)
        {
            return RawAPI.GRGraphicsCalcDrawFormatTextWidth(handle, Text, TextFormat.NativeHandle);
        }
        public double CalcLabelTextHeight(string Text)
        {
            return RawAPI.GRGraphicsCalcLabelTextHeight(handle, Text);
        }
        public double CalcLabelTextWidth(string Text)
        {
            return RawAPI.GRGraphicsCalcLabelTextWidth(handle, Text);
        }
        public int CalcTextOutLen(double Width, double Height, string Text, bool WordWrap)
        {
            return RawAPI.GRGraphicsCalcTextOutLen(handle, Width, Height, Text, WordWrap);
        }
        public void ClosePath()
        {
            RawAPI.GRGraphicsClosePath(handle);
        }
        public void CurveTo(double xInnerControl, double yInnerControl, double xOuterControl, double yOuterControl, double xEnd, double yEnd)
        {
            RawAPI.GRGraphicsCurveTo(handle, xInnerControl, yInnerControl, xOuterControl, yOuterControl, xEnd, yEnd);
        }
        public void DrawFormatText(string Text, double x, double y, double width, double height, TextFormat TextFormat)
        {
            RawAPI.GRGraphicsDrawFormatText(handle, Text, x, y, width, height, TextFormat.NativeHandle);
        }
        public void DrawFormatTextShrinkToFit(string Text, double x, double y, double width, double height, TextFormat TextFormat)
        {
            RawAPI.GRGraphicsDrawFormatTextShrinkToFit(handle, Text, x, y, width, height, TextFormat.NativeHandle);
        }
        public void DrawLabelText(string Text, double x, double y)
        {
            RawAPI.GRGraphicsDrawLabelText(handle, Text, x, y);
        }
        public void DrawPicture(Picture Picture, double left, double top, double width, double height, PictureAlignment PictureAlignment, PictureSizeMode PictureSizeMode, PictureTransparentMode Transparent)
        {
            RawAPI.GRGraphicsDrawPicture(handle, Picture.NativeHandle, left, top, width, height, PictureAlignment, PictureSizeMode, Transparent);
        }
        public void DrawPixel(double x, double y, uint Color)
        {
            RawAPI.GRGraphicsDrawPixel(handle, x, y, Color);
        }
        public void DrawPointMarker(PointMarkerStyle MarkerStyle, double cx, double cy, double size)
        {
            RawAPI.GRGraphicsDrawPointMarker(handle, MarkerStyle, cx, cy, size);
        }
        public void DrawRotateText(string Text, double x, double y, double TextAngle)
        {
            RawAPI.GRGraphicsDrawRotateText(handle, Text, x, y, TextAngle);
        }
        public void DrawText(string Text, double x, double y, double width, double Height, TextAlign TextAlign, bool WordWrap)
        {
            RawAPI.GRGraphicsDrawText(handle, Text, x, y, width, Height, TextAlign, WordWrap);
        }
        public void Ellipse(double x, double y, double width, double height, bool ToFill)
        {
            RawAPI.GRGraphicsEllipse(handle, x, y, width, height, ToFill);
        }
        public void EllipseArc(double left, double top, double width, double height, double BeginAngleDegree, double EndAngleDegree)
        {
            RawAPI.GRGraphicsEllipseArc(handle, left, top, width, height, BeginAngleDegree, EndAngleDegree);
        }
        public void EllipsePie(double left, double top, double width, double height, double BeginAngleDegree, double EndAngleDegree, bool ToFill)
        {
            RawAPI.GRGraphicsEllipsePie(handle, left, top, width, height, BeginAngleDegree, EndAngleDegree, ToFill);
        }
        public void EndPath()
        {
            RawAPI.GRGraphicsEndPath(handle);
        }
        public void FillPath()
        {
            RawAPI.GRGraphicsFillPath(handle);
        }
        public void FillRect(double x, double y, double width, double height, uint FillColor)
        {
            RawAPI.GRGraphicsFillRect(handle, x, y, width, height, FillColor);
        }
        public void FontPointChangeTo(double Point)
        {
            RawAPI.GRGraphicsFontPointChangeTo(handle, Point);
        }
        public void FontPointRestore()
        {
            RawAPI.GRGraphicsFontPointRestore(handle);
        }
        public void GradientFillRect(double x, double y, double width, double height, uint BeginColor, uint EndColor, GradientMode Mode)
        {
            RawAPI.GRGraphicsGradientFillRect(handle, x, y, width, height, BeginColor, EndColor, Mode);
        }
        public void LineTo(double x, double y)
        {
            RawAPI.GRGraphicsLineTo(handle, x, y);
        }
        public void MoveTo(double x, double y)
        {
            RawAPI.GRGraphicsMoveTo(handle, x, y);
        }
        public void Pie(double x, double y, double r, double BeginAngleDegree, double EndAngleDegree, bool ToFill)
        {
            RawAPI.GRGraphicsPie(handle, x, y, r, BeginAngleDegree, EndAngleDegree, ToFill);
        }
        public void PopClipRect()
        {
            RawAPI.GRGraphicsPopClipRect(handle);
        }
        public void PushClipRect(double x, double y, double width, double height)
        {
            RawAPI.GRGraphicsPushClipRect(handle, x, y, width, height);
        }
        public void Rectangle(double x, double y, double width, double height, bool ToFill)
        {
            RawAPI.GRGraphicsRectangle(handle, x, y, width, height, ToFill);
        }
        public void RestoreFillColor()
        {
            RawAPI.GRGraphicsRestoreFillColor(handle);
        }
        public void RestoreFont()
        {
            RawAPI.GRGraphicsRestoreFont(handle);
        }
        public void RestorePen()
        {
            RawAPI.GRGraphicsRestorePen(handle);
        }
        public void RestoreTextColor()
        {
            RawAPI.GRGraphicsRestoreTextColor(handle);
        }
        public void RoundRect(double x, double y, double width, double height, int cornerx, int cornery, bool ToFill)
        {
            RawAPI.GRGraphicsRoundRect(handle, x, y, width, height, cornerx, cornery, ToFill);
        }
        public void SelectFillColor(uint BrushColor)
        {
            RawAPI.GRGraphicsSelectFillColor(handle, BrushColor);
        }
        public void SelectFont(Font Font)
        {
            RawAPI.GRGraphicsSelectFont(handle, Font.NativeHandle);
        }
        public void SelectPen(double PenWidth, uint PenColor, PenStyle PenStyle)
        {
            RawAPI.GRGraphicsSelectPen(handle, PenWidth, PenColor, PenStyle);
        }
        public void SelectTextColor(uint TextColor)
        {
            RawAPI.GRGraphicsSelectTextColor(handle, TextColor);
        }
        public void StrokeAndFillPath()
        {
            RawAPI.GRGraphicsStrokeAndFillPath(handle);
        }
        public void StrokePath()
        {
            RawAPI.GRGraphicsStrokePath(handle);
        }
    }

    public class Object : Base
    {
        public Object(IntPtr _handle)
            : base(_handle)
        {
        }
        public LockType Lock
        {
            get
            {
                return RawAPI.GRObjectGetLock(handle);
            }
            set
            {
                RawAPI.GRObjectSetLock(handle, value);
            }
        }
        public ReportLinux OwnerReport
        {
            get
            {
                IntPtr theHandle = RawAPI.GRObjectGetOwnerReport(handle);
                return theHandle != IntPtr.Zero ? new ReportLinux(theHandle) : null;
            }
        }
        public Object Parent
        {
            get
            {
                IntPtr theHandle = RawAPI.GRObjectGetParent(handle);
                return theHandle != IntPtr.Zero ? new Object(theHandle) : null;
            }
        }
        public string Tag
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRObjectGetTag(handle));
            }
            set
            {
                RawAPI.GRObjectSetTag(handle, value);
            }
        }

        public void Assign(Object Obj)
        {
            RawAPI.GRObjectAssign(handle, Obj.NativeHandle);
        }
    }

    public class Parameter : Object
    {
        public Parameter(IntPtr _handle)
            : base(_handle)
        {
        }
        public bool AsBoolean
        {
            get
            {
                return RawAPI.GRParameterGetAsBoolean(handle);
            }
            set
            {
                RawAPI.GRParameterSetAsBoolean(handle, value);
            }
        }
        public long AsDateTime
        {
            get
            {
                return RawAPI.GRParameterGetAsDateTime(handle);
            }
            set
            {
                RawAPI.GRParameterSetAsDateTime(handle, value);
            }
        }
        public double AsFloat
        {
            get
            {
                return RawAPI.GRParameterGetAsFloat(handle);
            }
            set
            {
                RawAPI.GRParameterSetAsFloat(handle, value);
            }
        }
        public int AsInteger
        {
            get
            {
                return RawAPI.GRParameterGetAsInteger(handle);
            }
            set
            {
                RawAPI.GRParameterSetAsInteger(handle, value);
            }
        }
        public string AsString
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRParameterGetAsString(handle));
            }
            set
            {
                RawAPI.GRParameterSetAsString(handle, value);
            }
        }
        public ParameterDataType DataType
        {
            get
            {
                return RawAPI.GRParameterGetDataType(handle);
            }
            set
            {
                RawAPI.GRParameterSetDataType(handle, value);
            }
        }
        public string DisplayText
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRParameterGetDisplayText(handle));
            }
        }
        public string Format
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRParameterGetFormat(handle));
            }
            set
            {
                RawAPI.GRParameterSetFormat(handle, value);
            }
        }
        public bool IsNull
        {
            get
            {
                return RawAPI.GRParameterGetIsNull(handle);
            }
        }
        public string Name
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRParameterGetName(handle));
            }
            set
            {
                RawAPI.GRParameterSetName(handle, value);
            }
        }
        public string Value
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRParameterGetValue(handle));
            }
            set
            {
                RawAPI.GRParameterSetValue(handle, value);
            }
        }

        public void Clear()
        {
            RawAPI.GRParameterClear(handle);
        }
    }

    public class Section : Object
    {
        public Section(IntPtr _handle)
            : base(_handle)
        {
        }
        public uint BackColor
        {
            get
            {
                return RawAPI.GRSectionGetBackColor(handle);
            }
            set
            {
                RawAPI.GRSectionSetBackColor(handle, value);
            }
        }
        public string BookmarkText
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRSectionGetBookmarkText(handle));
            }
            set
            {
                RawAPI.GRSectionSetBookmarkText(handle, value);
            }
        }
        public bool CanGrow
        {
            get
            {
                return RawAPI.GRSectionGetCanGrow(handle);
            }
            set
            {
                RawAPI.GRSectionSetCanGrow(handle, value);
            }
        }
        public bool CanShrink
        {
            get
            {
                return RawAPI.GRSectionGetCanShrink(handle);
            }
            set
            {
                RawAPI.GRSectionSetCanShrink(handle, value);
            }
        }
        public Controls Controls
        {
            get
            {
                IntPtr theHandle = RawAPI.GRSectionGetControls(handle);
                return theHandle != IntPtr.Zero ? new Controls(theHandle) : null;
            }
        }
        public DisplayCursor Cursor
        {
            get
            {
                return RawAPI.GRSectionGetCursor(handle);
            }
            set
            {
                RawAPI.GRSectionSetCursor(handle, value);
            }
        }
        public Font Font
        {
            get
            {
                IntPtr theHandle = RawAPI.GRSectionGetFont(handle);
                return theHandle != IntPtr.Zero ? new Font(theHandle) : null;
            }
        }
        public string FormatScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRSectionGetFormatScript(handle));
            }
            set
            {
                RawAPI.GRSectionSetFormatScript(handle, value);
            }
        }
        public double Height
        {
            get
            {
                return RawAPI.GRSectionGetHeight(handle);
            }
            set
            {
                RawAPI.GRSectionSetHeight(handle, value);
            }
        }
        public bool KeepTogether
        {
            get
            {
                return RawAPI.GRSectionGetKeepTogether(handle);
            }
            set
            {
                RawAPI.GRSectionSetKeepTogether(handle, value);
            }
        }
        public SectionType SectionType
        {
            get
            {
                return RawAPI.GRSectionGetSectionType(handle);
            }
        }
        public bool Visible
        {
            get
            {
                return RawAPI.GRSectionGetVisible(handle);
            }
            set
            {
                RawAPI.GRSectionSetVisible(handle, value);
            }
        }

    }

    public class CellBase : Object
    {
        public CellBase(IntPtr _handle)
            : base(_handle)
        {
        }
        public uint BackColor
        {
            get
            {
                return RawAPI.GRCellBaseGetBackColor(handle);
            }
            set
            {
                RawAPI.GRCellBaseSetBackColor(handle, value);
            }
        }
        public Border Border
        {
            get
            {
                IntPtr theHandle = RawAPI.GRCellBaseGetBorder(handle);
                return theHandle != IntPtr.Zero ? new Border(theHandle) : null;
            }
        }
        public bool BorderCustom
        {
            get
            {
                return RawAPI.GRCellBaseGetBorderCustom(handle);
            }
            set
            {
                RawAPI.GRCellBaseSetBorderCustom(handle, value);
            }
        }
        public bool CanGrow
        {
            get
            {
                return RawAPI.GRCellBaseGetCanGrow(handle);
            }
            set
            {
                RawAPI.GRCellBaseSetCanGrow(handle, value);
            }
        }
        public bool CanShrink
        {
            get
            {
                return RawAPI.GRCellBaseGetCanShrink(handle);
            }
            set
            {
                RawAPI.GRCellBaseSetCanShrink(handle, value);
            }
        }
        public Controls Controls
        {
            get
            {
                IntPtr theHandle = RawAPI.GRCellBaseGetControls(handle);
                return theHandle != IntPtr.Zero ? new Controls(theHandle) : null;
            }
        }
        public DisplayCursor Cursor
        {
            get
            {
                return RawAPI.GRCellBaseGetCursor(handle);
            }
            set
            {
                RawAPI.GRCellBaseSetCursor(handle, value);
            }
        }
        public string DisplayText
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRCellBaseGetDisplayText(handle));
            }
            set
            {
                RawAPI.GRCellBaseSetDisplayText(handle, value);
            }
        }
        public Font Font
        {
            get
            {
                IntPtr theHandle = RawAPI.GRCellBaseGetFont(handle);
                return theHandle != IntPtr.Zero ? new Font(theHandle) : null;
            }
        }
        public uint ForeColor
        {
            get
            {
                return RawAPI.GRCellBaseGetForeColor(handle);
            }
            set
            {
                RawAPI.GRCellBaseSetForeColor(handle, value);
            }
        }
        public bool FreeCell
        {
            get
            {
                return RawAPI.GRCellBaseGetFreeCell(handle);
            }
            set
            {
                RawAPI.GRCellBaseSetFreeCell(handle, value);
            }
        }
        public string GetDisplayTextScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRCellBaseGetGetDisplayTextScript(handle));
            }
            set
            {
                RawAPI.GRCellBaseSetGetDisplayTextScript(handle, value);
            }
        }
        public double PaddingBottom
        {
            get
            {
                return RawAPI.GRCellBaseGetPaddingBottom(handle);
            }
            set
            {
                RawAPI.GRCellBaseSetPaddingBottom(handle, value);
            }
        }
        public double PaddingLeft
        {
            get
            {
                return RawAPI.GRCellBaseGetPaddingLeft(handle);
            }
            set
            {
                RawAPI.GRCellBaseSetPaddingLeft(handle, value);
            }
        }
        public double PaddingRight
        {
            get
            {
                return RawAPI.GRCellBaseGetPaddingRight(handle);
            }
            set
            {
                RawAPI.GRCellBaseSetPaddingRight(handle, value);
            }
        }
        public double PaddingTop
        {
            get
            {
                return RawAPI.GRCellBaseGetPaddingTop(handle);
            }
            set
            {
                RawAPI.GRCellBaseSetPaddingTop(handle, value);
            }
        }
        public PrintType PrintType
        {
            get
            {
                return RawAPI.GRCellBaseGetPrintType(handle);
            }
            set
            {
                RawAPI.GRCellBaseSetPrintType(handle, value);
            }
        }
        public bool ShowMoneyLabel
        {
            get
            {
                return RawAPI.GRCellBaseGetShowMoneyLabel(handle);
            }
            set
            {
                RawAPI.GRCellBaseSetShowMoneyLabel(handle, value);
            }
        }
        public bool ShowMoneyLine
        {
            get
            {
                return RawAPI.GRCellBaseGetShowMoneyLine(handle);
            }
            set
            {
                RawAPI.GRCellBaseSetShowMoneyLine(handle, value);
            }
        }
        public bool ShrinkFontToFit
        {
            get
            {
                return RawAPI.GRCellBaseGetShrinkFontToFit(handle);
            }
            set
            {
                RawAPI.GRCellBaseSetShrinkFontToFit(handle, value);
            }
        }
        public TextFormat TextFormat
        {
            get
            {
                IntPtr theHandle = RawAPI.GRCellBaseGetTextFormat(handle);
                return theHandle != IntPtr.Zero ? new TextFormat(theHandle) : null;
            }
        }

    }

    public class Control : Object
    {
        public Control(IntPtr _handle)
            : base(_handle)
        {
        }
        public string AlignColumn
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRControlGetAlignColumn(handle));
            }
            set
            {
                RawAPI.GRControlSetAlignColumn(handle, value);
            }
        }
        public string AlignColumnEx
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRControlGetAlignColumnEx(handle));
            }
            set
            {
                RawAPI.GRControlSetAlignColumnEx(handle, value);
            }
        }
        public AlignColumnStyle AlignColumnSide
        {
            get
            {
                return RawAPI.GRControlGetAlignColumnSide(handle);
            }
            set
            {
                RawAPI.GRControlSetAlignColumnSide(handle, value);
            }
        }
        public int Anchor
        {
            get
            {
                return RawAPI.GRControlGetAnchor(handle);
            }
            set
            {
                RawAPI.GRControlSetAnchor(handle, value);
            }
        }
        public Barcode AsBarcode
        {
            get
            {
                IntPtr theHandle = RawAPI.GRControlGetAsBarcode(handle);
                return theHandle != IntPtr.Zero ? new Barcode(theHandle) : null;
            }
        }
        public Chart AsChart
        {
            get
            {
                IntPtr theHandle = RawAPI.GRControlGetAsChart(handle);
                return theHandle != IntPtr.Zero ? new Chart(theHandle) : null;
            }
        }
        public FieldBox AsFieldBox
        {
            get
            {
                IntPtr theHandle = RawAPI.GRControlGetAsFieldBox(handle);
                return theHandle != IntPtr.Zero ? new FieldBox(theHandle) : null;
            }
        }
        public FreeGrid AsFreeGrid
        {
            get
            {
                IntPtr theHandle = RawAPI.GRControlGetAsFreeGrid(handle);
                return theHandle != IntPtr.Zero ? new FreeGrid(theHandle) : null;
            }
        }
        public Line AsLine
        {
            get
            {
                IntPtr theHandle = RawAPI.GRControlGetAsLine(handle);
                return theHandle != IntPtr.Zero ? new Line(theHandle) : null;
            }
        }
        public MemoBox AsMemoBox
        {
            get
            {
                IntPtr theHandle = RawAPI.GRControlGetAsMemoBox(handle);
                return theHandle != IntPtr.Zero ? new MemoBox(theHandle) : null;
            }
        }
        public PictureBox AsPictureBox
        {
            get
            {
                IntPtr theHandle = RawAPI.GRControlGetAsPictureBox(handle);
                return theHandle != IntPtr.Zero ? new PictureBox(theHandle) : null;
            }
        }
        public RichTextBox AsRichTextBox
        {
            get
            {
                IntPtr theHandle = RawAPI.GRControlGetAsRichTextBox(handle);
                return theHandle != IntPtr.Zero ? new RichTextBox(theHandle) : null;
            }
        }
        public ShapeBox AsShapeBox
        {
            get
            {
                IntPtr theHandle = RawAPI.GRControlGetAsShapeBox(handle);
                return theHandle != IntPtr.Zero ? new ShapeBox(theHandle) : null;
            }
        }
        public StaticBox AsStaticBox
        {
            get
            {
                IntPtr theHandle = RawAPI.GRControlGetAsStaticBox(handle);
                return theHandle != IntPtr.Zero ? new StaticBox(theHandle) : null;
            }
        }
        public SubReport AsSubReport
        {
            get
            {
                IntPtr theHandle = RawAPI.GRControlGetAsSubReport(handle);
                return theHandle != IntPtr.Zero ? new SubReport(theHandle) : null;
            }
        }
        public SummaryBox AsSummaryBox
        {
            get
            {
                IntPtr theHandle = RawAPI.GRControlGetAsSummaryBox(handle);
                return theHandle != IntPtr.Zero ? new SummaryBox(theHandle) : null;
            }
        }
        public SystemVarBox AsSystemVarBox
        {
            get
            {
                IntPtr theHandle = RawAPI.GRControlGetAsSystemVarBox(handle);
                return theHandle != IntPtr.Zero ? new SystemVarBox(theHandle) : null;
            }
        }
        public TextBox AsTextBox
        {
            get
            {
                IntPtr theHandle = RawAPI.GRControlGetAsTextBox(handle);
                return theHandle != IntPtr.Zero ? new TextBox(theHandle) : null;
            }
        }
        public uint BackColor
        {
            get
            {
                return RawAPI.GRControlGetBackColor(handle);
            }
            set
            {
                RawAPI.GRControlSetBackColor(handle, value);
            }
        }
        public BackStyle BackStyle
        {
            get
            {
                return RawAPI.GRControlGetBackStyle(handle);
            }
            set
            {
                RawAPI.GRControlSetBackStyle(handle, value);
            }
        }
        public Border Border
        {
            get
            {
                IntPtr theHandle = RawAPI.GRControlGetBorder(handle);
                return theHandle != IntPtr.Zero ? new Border(theHandle) : null;
            }
        }
        public CenterStyle Center
        {
            get
            {
                return RawAPI.GRControlGetCenter(handle);
            }
            set
            {
                RawAPI.GRControlSetCenter(handle, value);
            }
        }
        public ControlType ControlType
        {
            get
            {
                return RawAPI.GRControlGetControlType(handle);
            }
        }
        public DisplayCursor Cursor
        {
            get
            {
                return RawAPI.GRControlGetCursor(handle);
            }
            set
            {
                RawAPI.GRControlSetCursor(handle, value);
            }
        }
        public bool CustomDraw
        {
            get
            {
                return RawAPI.GRControlGetCustomDraw(handle);
            }
            set
            {
                RawAPI.GRControlSetCustomDraw(handle, value);
            }
        }
        public string CustomDrawScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRControlGetCustomDrawScript(handle));
            }
            set
            {
                RawAPI.GRControlSetCustomDrawScript(handle, value);
            }
        }
        public DockStyle Dock
        {
            get
            {
                return RawAPI.GRControlGetDock(handle);
            }
            set
            {
                RawAPI.GRControlSetDock(handle, value);
            }
        }
        public Font Font
        {
            get
            {
                IntPtr theHandle = RawAPI.GRControlGetFont(handle);
                return theHandle != IntPtr.Zero ? new Font(theHandle) : null;
            }
        }
        public uint ForeColor
        {
            get
            {
                return RawAPI.GRControlGetForeColor(handle);
            }
            set
            {
                RawAPI.GRControlSetForeColor(handle, value);
            }
        }
        public double Height
        {
            get
            {
                return RawAPI.GRControlGetHeight(handle);
            }
            set
            {
                RawAPI.GRControlSetHeight(handle, value);
            }
        }
        public double Left
        {
            get
            {
                return RawAPI.GRControlGetLeft(handle);
            }
            set
            {
                RawAPI.GRControlSetLeft(handle, value);
            }
        }
        public string Name
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRControlGetName(handle));
            }
            set
            {
                RawAPI.GRControlSetName(handle, value);
            }
        }
        public double PaddingBottom
        {
            get
            {
                return RawAPI.GRControlGetPaddingBottom(handle);
            }
            set
            {
                RawAPI.GRControlSetPaddingBottom(handle, value);
            }
        }
        public double PaddingLeft
        {
            get
            {
                return RawAPI.GRControlGetPaddingLeft(handle);
            }
            set
            {
                RawAPI.GRControlSetPaddingLeft(handle, value);
            }
        }
        public double PaddingRight
        {
            get
            {
                return RawAPI.GRControlGetPaddingRight(handle);
            }
            set
            {
                RawAPI.GRControlSetPaddingRight(handle, value);
            }
        }
        public double PaddingTop
        {
            get
            {
                return RawAPI.GRControlGetPaddingTop(handle);
            }
            set
            {
                RawAPI.GRControlSetPaddingTop(handle, value);
            }
        }
        public PrintType PrintType
        {
            get
            {
                return RawAPI.GRControlGetPrintType(handle);
            }
            set
            {
                RawAPI.GRControlSetPrintType(handle, value);
            }
        }
        public ShiftMode ShiftMode
        {
            get
            {
                return RawAPI.GRControlGetShiftMode(handle);
            }
            set
            {
                RawAPI.GRControlSetShiftMode(handle, value);
            }
        }
        public double Top
        {
            get
            {
                return RawAPI.GRControlGetTop(handle);
            }
            set
            {
                RawAPI.GRControlSetTop(handle, value);
            }
        }
        public bool Visible
        {
            get
            {
                return RawAPI.GRControlGetVisible(handle);
            }
            set
            {
                RawAPI.GRControlSetVisible(handle, value);
            }
        }
        public double Width
        {
            get
            {
                return RawAPI.GRControlGetWidth(handle);
            }
            set
            {
                RawAPI.GRControlSetWidth(handle, value);
            }
        }

        public void BringToFront()
        {
            RawAPI.GRControlBringToFront(handle);
        }
        public void DrawDefault()
        {
            RawAPI.GRControlDrawDefault(handle);
        }
        public bool GetAnchorSide(AnchorStyles Side)
        {
            return RawAPI.GRControlGetAnchorSide(handle, Side);
        }
        public void SendToBack()
        {
            RawAPI.GRControlSendToBack(handle);
        }
        public void SetAnchorSide(AnchorStyles Side, bool ToAnchor)
        {
            RawAPI.GRControlSetAnchorSide(handle, Side, ToAnchor);
        }
        public void SetBounds(double left, double Top, double Right, double Bottom)
        {
            RawAPI.GRControlSetBounds(handle, left, Top, Right, Bottom);
        }
    }

    public class TextBox : Control
    {
        public TextBox(IntPtr _handle)
            : base(_handle)
        {
        }
        public bool CanGrow
        {
            get
            {
                return RawAPI.GRTextBoxGetCanGrow(handle);
            }
            set
            {
                RawAPI.GRTextBoxSetCanGrow(handle, value);
            }
        }
        public bool CanShrink
        {
            get
            {
                return RawAPI.GRTextBoxGetCanShrink(handle);
            }
            set
            {
                RawAPI.GRTextBoxSetCanShrink(handle, value);
            }
        }
        public string DisplayText
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRTextBoxGetDisplayText(handle));
            }
            set
            {
                RawAPI.GRTextBoxSetDisplayText(handle, value);
            }
        }
        public string GetDisplayTextScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRTextBoxGetGetDisplayTextScript(handle));
            }
            set
            {
                RawAPI.GRTextBoxSetGetDisplayTextScript(handle, value);
            }
        }
        public bool ShowMoneyLabel
        {
            get
            {
                return RawAPI.GRTextBoxGetShowMoneyLabel(handle);
            }
            set
            {
                RawAPI.GRTextBoxSetShowMoneyLabel(handle, value);
            }
        }
        public bool ShowMoneyLine
        {
            get
            {
                return RawAPI.GRTextBoxGetShowMoneyLine(handle);
            }
            set
            {
                RawAPI.GRTextBoxSetShowMoneyLine(handle, value);
            }
        }
        public bool ShrinkFontToFit
        {
            get
            {
                return RawAPI.GRTextBoxGetShrinkFontToFit(handle);
            }
            set
            {
                RawAPI.GRTextBoxSetShrinkFontToFit(handle, value);
            }
        }
        public TextFormat TextFormat
        {
            get
            {
                IntPtr theHandle = RawAPI.GRTextBoxGetTextFormat(handle);
                return theHandle != IntPtr.Zero ? new TextFormat(theHandle) : null;
            }
        }

    }

    public class StaticBox : TextBox
    {
        public StaticBox(IntPtr _handle)
            : base(_handle)
        {
        }
        public string Parameter
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRStaticBoxGetParameter(handle));
            }
            set
            {
                RawAPI.GRStaticBoxSetParameter(handle, value);
            }
        }
        public string Text
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRStaticBoxGetText(handle));
            }
            set
            {
                RawAPI.GRStaticBoxSetText(handle, value);
            }
        }

    }

    public class FieldBox : TextBox
    {
        public FieldBox(IntPtr _handle)
            : base(_handle)
        {
        }
        public string DataField
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRFieldBoxGetDataField(handle));
            }
            set
            {
                RawAPI.GRFieldBoxSetDataField(handle, value);
            }
        }

    }

    public class SummaryBox : TextBox
    {
        public SummaryBox(IntPtr _handle)
            : base(_handle)
        {
        }
        public string DataField
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRSummaryBoxGetDataField(handle));
            }
            set
            {
                RawAPI.GRSummaryBoxSetDataField(handle, value);
            }
        }
        public string DisplayField
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRSummaryBoxGetDisplayField(handle));
            }
            set
            {
                RawAPI.GRSummaryBoxSetDisplayField(handle, value);
            }
        }
        public string Format
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRSummaryBoxGetFormat(handle));
            }
            set
            {
                RawAPI.GRSummaryBoxSetFormat(handle, value);
            }
        }
        public int RankNo
        {
            get
            {
                return RawAPI.GRSummaryBoxGetRankNo(handle);
            }
            set
            {
                RawAPI.GRSummaryBoxSetRankNo(handle, value);
            }
        }
        public SummaryFun SummaryFun
        {
            get
            {
                return RawAPI.GRSummaryBoxGetSummaryFun(handle);
            }
            set
            {
                RawAPI.GRSummaryBoxSetSummaryFun(handle, value);
            }
        }
        public double Value
        {
            get
            {
                return RawAPI.GRSummaryBoxGetValue(handle);
            }
            set
            {
                RawAPI.GRSummaryBoxSetValue(handle, value);
            }
        }

    }

    public class SystemVarBox : TextBox
    {
        public SystemVarBox(IntPtr _handle)
            : base(_handle)
        {
        }
        public string Format
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRSystemVarBoxGetFormat(handle));
            }
            set
            {
                RawAPI.GRSystemVarBoxSetFormat(handle, value);
            }
        }
        public int GroupIndex
        {
            get
            {
                return RawAPI.GRSystemVarBoxGetGroupIndex(handle);
            }
            set
            {
                RawAPI.GRSystemVarBoxSetGroupIndex(handle, value);
            }
        }
        public SystemVarType SystemVar
        {
            get
            {
                return RawAPI.GRSystemVarBoxGetSystemVar(handle);
            }
            set
            {
                RawAPI.GRSystemVarBoxSetSystemVar(handle, value);
            }
        }

    }

    public class MemoBox : TextBox
    {
        public MemoBox(IntPtr _handle)
            : base(_handle)
        {
        }
        public double AsFloat
        {
            get
            {
                return RawAPI.GRMemoBoxGetAsFloat(handle);
            }
        }
        public string FlowTo
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRMemoBoxGetFlowTo(handle));
            }
            set
            {
                RawAPI.GRMemoBoxSetFlowTo(handle, value);
            }
        }
        public string Text
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRMemoBoxGetText(handle));
            }
            set
            {
                RawAPI.GRMemoBoxSetText(handle, value);
            }
        }

    }

    public class ShapeBox : Control
    {
        public ShapeBox(IntPtr _handle)
            : base(_handle)
        {
        }
        public int CornerDx
        {
            get
            {
                return RawAPI.GRShapeBoxGetCornerDx(handle);
            }
            set
            {
                RawAPI.GRShapeBoxSetCornerDx(handle, value);
            }
        }
        public int CornerDy
        {
            get
            {
                return RawAPI.GRShapeBoxGetCornerDy(handle);
            }
            set
            {
                RawAPI.GRShapeBoxSetCornerDy(handle, value);
            }
        }
        public uint FillColor
        {
            get
            {
                return RawAPI.GRShapeBoxGetFillColor(handle);
            }
            set
            {
                RawAPI.GRShapeBoxSetFillColor(handle, value);
            }
        }
        public BackStyle FillStyle
        {
            get
            {
                return RawAPI.GRShapeBoxGetFillStyle(handle);
            }
            set
            {
                RawAPI.GRShapeBoxSetFillStyle(handle, value);
            }
        }
        public Pen LinePen
        {
            get
            {
                IntPtr theHandle = RawAPI.GRShapeBoxGetLinePen(handle);
                return theHandle != IntPtr.Zero ? new Pen(theHandle) : null;
            }
        }
        public ShapeType ShapeType
        {
            get
            {
                return RawAPI.GRShapeBoxGetShapeType(handle);
            }
            set
            {
                RawAPI.GRShapeBoxSetShapeType(handle, value);
            }
        }

    }

    public class PictureBox : Control
    {
        public PictureBox(IntPtr _handle)
            : base(_handle)
        {
        }
        public PictureAlignment Alignment
        {
            get
            {
                return RawAPI.GRPictureBoxGetAlignment(handle);
            }
            set
            {
                RawAPI.GRPictureBoxSetAlignment(handle, value);
            }
        }
        public string DataField
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRPictureBoxGetDataField(handle));
            }
            set
            {
                RawAPI.GRPictureBoxSetDataField(handle, value);
            }
        }
        public string ImageFile
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRPictureBoxGetImageFile(handle));
            }
            set
            {
                RawAPI.GRPictureBoxSetImageFile(handle, value);
            }
        }
        public int ImageIndex
        {
            get
            {
                return RawAPI.GRPictureBoxGetImageIndex(handle);
            }
            set
            {
                RawAPI.GRPictureBoxSetImageIndex(handle, value);
            }
        }
        public Picture Picture
        {
            get
            {
                IntPtr theHandle = RawAPI.GRPictureBoxGetPicture(handle);
                return theHandle != IntPtr.Zero ? new Picture(theHandle) : null;
            }
            set
            {
                RawAPI.GRPictureBoxSetPicture(handle, value != null ? value.NativeHandle : IntPtr.Zero);
            }
        }
        public PictureRotateMode RotateMode
        {
            get
            {
                return RawAPI.GRPictureBoxGetRotateMode(handle);
            }
            set
            {
                RawAPI.GRPictureBoxSetRotateMode(handle, value);
            }
        }
        public PictureSizeMode SizeMode
        {
            get
            {
                return RawAPI.GRPictureBoxGetSizeMode(handle);
            }
            set
            {
                RawAPI.GRPictureBoxSetSizeMode(handle, value);
            }
        }
        public PictureTransparentMode TransparentMode
        {
            get
            {
                return RawAPI.GRPictureBoxGetTransparentMode(handle);
            }
            set
            {
                RawAPI.GRPictureBoxSetTransparentMode(handle, value);
            }
        }

        public void AttachSystemImage(SystemImage SystemImage)
        {
            RawAPI.GRPictureBoxAttachSystemImage(handle, SystemImage);
        }
        public bool LoadFromBinary(BinaryObject BinaryObject)
        {
            return RawAPI.GRPictureBoxLoadFromBinary(handle, BinaryObject.NativeHandle);
        }
        public bool LoadFromFile(string PathFile)
        {
            return RawAPI.GRPictureBoxLoadFromFile(handle, PathFile);
        }
        public bool LoadFromMemory(IntPtr pBuffer, int BytesCount)
        {
            return RawAPI.GRPictureBoxLoadFromMemory(handle, pBuffer, BytesCount);
        }
    }

    public class RichTextBox : Control
    {
        public RichTextBox(IntPtr _handle)
            : base(_handle)
        {
        }
        public bool CanGrow
        {
            get
            {
                return RawAPI.GRRichTextBoxGetCanGrow(handle);
            }
            set
            {
                RawAPI.GRRichTextBoxSetCanGrow(handle, value);
            }
        }
        public bool CanShrink
        {
            get
            {
                return RawAPI.GRRichTextBoxGetCanShrink(handle);
            }
            set
            {
                RawAPI.GRRichTextBoxSetCanShrink(handle, value);
            }
        }
        public string DataField
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRRichTextBoxGetDataField(handle));
            }
            set
            {
                RawAPI.GRRichTextBoxSetDataField(handle, value);
            }
        }
        public string RTF
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRRichTextBoxGetRTF(handle));
            }
            set
            {
                RawAPI.GRRichTextBoxSetRTF(handle, value);
            }
        }

        public bool LoadFromFile(string PathFile)
        {
            return RawAPI.GRRichTextBoxLoadFromFile(handle, PathFile);
        }
        public bool LoadFromMemory(IntPtr pBuffer, int BytesCount)
        {
            return RawAPI.GRRichTextBoxLoadFromMemory(handle, pBuffer, BytesCount);
        }
    }

    public class SubReport : Control
    {
        public SubReport(IntPtr _handle)
            : base(_handle)
        {
        }
        public bool CanGrow
        {
            get
            {
                return RawAPI.GRSubReportGetCanGrow(handle);
            }
            set
            {
                RawAPI.GRSubReportSetCanGrow(handle, value);
            }
        }
        public bool CanShrink
        {
            get
            {
                return RawAPI.GRSubReportGetCanShrink(handle);
            }
            set
            {
                RawAPI.GRSubReportSetCanShrink(handle, value);
            }
        }
        public bool HideOnRecordsetEmpty
        {
            get
            {
                return RawAPI.GRSubReportGetHideOnRecordsetEmpty(handle);
            }
            set
            {
                RawAPI.GRSubReportSetHideOnRecordsetEmpty(handle, value);
            }
        }
        public bool ParentPageSettings
        {
            get
            {
                return RawAPI.GRSubReportGetParentPageSettings(handle);
            }
            set
            {
                RawAPI.GRSubReportSetParentPageSettings(handle, value);
            }
        }
        public string RelationFields
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRSubReportGetRelationFields(handle));
            }
            set
            {
                RawAPI.GRSubReportSetRelationFields(handle, value);
            }
        }
        public ReportLinux Report
        {
            get
            {
                IntPtr theHandle = RawAPI.GRSubReportGetReport(handle);
                return theHandle != IntPtr.Zero ? new ReportLinux(theHandle) : null;
            }
        }
        public string ReportFile
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRSubReportGetReportFile(handle));
            }
            set
            {
                RawAPI.GRSubReportSetReportFile(handle, value);
            }
        }
        public bool ResetPageNumber
        {
            get
            {
                return RawAPI.GRSubReportGetResetPageNumber(handle);
            }
            set
            {
                RawAPI.GRSubReportSetResetPageNumber(handle, value);
            }
        }
        public bool ToNewExcelSheet
        {
            get
            {
                return RawAPI.GRSubReportGetToNewExcelSheet(handle);
            }
            set
            {
                RawAPI.GRSubReportSetToNewExcelSheet(handle, value);
            }
        }

    }

    public class Line : Control
    {
        public Line(IntPtr _handle)
            : base(_handle)
        {
        }
        public Pen LinePen
        {
            get
            {
                IntPtr theHandle = RawAPI.GRLineGetLinePen(handle);
                return theHandle != IntPtr.Zero ? new Pen(theHandle) : null;
            }
        }
        public double X1
        {
            get
            {
                return RawAPI.GRLineGetX1(handle);
            }
            set
            {
                RawAPI.GRLineSetX1(handle, value);
            }
        }
        public double X2
        {
            get
            {
                return RawAPI.GRLineGetX2(handle);
            }
            set
            {
                RawAPI.GRLineSetX2(handle, value);
            }
        }
        public double Y1
        {
            get
            {
                return RawAPI.GRLineGetY1(handle);
            }
            set
            {
                RawAPI.GRLineSetY1(handle, value);
            }
        }
        public double Y2
        {
            get
            {
                return RawAPI.GRLineGetY2(handle);
            }
            set
            {
                RawAPI.GRLineSetY2(handle, value);
            }
        }

    }

    public class Barcode : Control
    {
        public Barcode(IntPtr _handle)
            : base(_handle)
        {
        }
        public StringAlignment Alignment
        {
            get
            {
                return RawAPI.GRBarcodeGetAlignment(handle);
            }
            set
            {
                RawAPI.GRBarcodeSetAlignment(handle, value);
            }
        }
        public BarcodeType BarcodeType
        {
            get
            {
                return RawAPI.GRBarcodeGetBarcodeType(handle);
            }
            set
            {
                RawAPI.GRBarcodeSetBarcodeType(handle, value);
            }
        }
        public int BarDPI
        {
            get
            {
                return RawAPI.GRBarcodeGetBarDPI(handle);
            }
            set
            {
                RawAPI.GRBarcodeSetBarDPI(handle, value);
            }
        }
        public double BarRatio
        {
            get
            {
                return RawAPI.GRBarcodeGetBarRatio(handle);
            }
            set
            {
                RawAPI.GRBarcodeSetBarRatio(handle, value);
            }
        }
        public double BarWidth
        {
            get
            {
                return RawAPI.GRBarcodeGetBarWidth(handle);
            }
            set
            {
                RawAPI.GRBarcodeSetBarWidth(handle, value);
            }
        }
        public StringAlignment CaptionAlignment
        {
            get
            {
                return RawAPI.GRBarcodeGetCaptionAlignment(handle);
            }
            set
            {
                RawAPI.GRBarcodeSetCaptionAlignment(handle, value);
            }
        }
        public BarcodeCaptionPosition CaptionPosition
        {
            get
            {
                return RawAPI.GRBarcodeGetCaptionPosition(handle);
            }
            set
            {
                RawAPI.GRBarcodeSetCaptionPosition(handle, value);
            }
        }
        public bool CheckSum
        {
            get
            {
                return RawAPI.GRBarcodeGetCheckSum(handle);
            }
            set
            {
                RawAPI.GRBarcodeSetCheckSum(handle, value);
            }
        }
        public BarcodeDirection Direction
        {
            get
            {
                return RawAPI.GRBarcodeGetDirection(handle);
            }
            set
            {
                RawAPI.GRBarcodeSetDirection(handle, value);
            }
        }
        public string DisplayText
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRBarcodeGetDisplayText(handle));
            }
        }
        public DtmxEncoding DtmxEncoding
        {
            get
            {
                return RawAPI.GRBarcodeGetDtmxEncoding(handle);
            }
            set
            {
                RawAPI.GRBarcodeSetDtmxEncoding(handle, value);
            }
        }
        public DtmxMatrixSize DtmxMatrixSize
        {
            get
            {
                return RawAPI.GRBarcodeGetDtmxMatrixSize(handle);
            }
            set
            {
                RawAPI.GRBarcodeSetDtmxMatrixSize(handle, value);
            }
        }
        public int PDF417Columns
        {
            get
            {
                return RawAPI.GRBarcodeGetPDF417Columns(handle);
            }
            set
            {
                RawAPI.GRBarcodeSetPDF417Columns(handle, value);
            }
        }
        public int PDF417ErrorLevel
        {
            get
            {
                return RawAPI.GRBarcodeGetPDF417ErrorLevel(handle);
            }
            set
            {
                RawAPI.GRBarcodeSetPDF417ErrorLevel(handle, value);
            }
        }
        public int PDF417Rows
        {
            get
            {
                return RawAPI.GRBarcodeGetPDF417Rows(handle);
            }
            set
            {
                RawAPI.GRBarcodeSetPDF417Rows(handle, value);
            }
        }
        public bool PDF417Simple
        {
            get
            {
                return RawAPI.GRBarcodeGetPDF417Simple(handle);
            }
            set
            {
                RawAPI.GRBarcodeSetPDF417Simple(handle, value);
            }
        }
        public int QRCodeErrorLevel
        {
            get
            {
                return RawAPI.GRBarcodeGetQRCodeErrorLevel(handle);
            }
            set
            {
                RawAPI.GRBarcodeSetQRCodeErrorLevel(handle, value);
            }
        }
        public int QRCodeMask
        {
            get
            {
                return RawAPI.GRBarcodeGetQRCodeMask(handle);
            }
            set
            {
                RawAPI.GRBarcodeSetQRCodeMask(handle, value);
            }
        }
        public int QRCodeVersion
        {
            get
            {
                return RawAPI.GRBarcodeGetQRCodeVersion(handle);
            }
            set
            {
                RawAPI.GRBarcodeSetQRCodeVersion(handle, value);
            }
        }
        public string Text
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRBarcodeGetText(handle));
            }
            set
            {
                RawAPI.GRBarcodeSetText(handle, value);
            }
        }

    }

    public class ChartAxis : Object
    {
        public ChartAxis(IntPtr _handle)
            : base(_handle)
        {
        }
        public Pen CoordLinePen
        {
            get
            {
                IntPtr theHandle = RawAPI.GRChartAxisGetCoordLinePen(handle);
                return theHandle != IntPtr.Zero ? new Pen(theHandle) : null;
            }
        }
        public bool CoordLineVisible
        {
            get
            {
                return RawAPI.GRChartAxisGetCoordLineVisible(handle);
            }
            set
            {
                RawAPI.GRChartAxisSetCoordLineVisible(handle, value);
            }
        }
        public bool DateTimeAxis
        {
            get
            {
                return RawAPI.GRChartAxisGetDateTimeAxis(handle);
            }
            set
            {
                RawAPI.GRChartAxisSetDateTimeAxis(handle, value);
            }
        }
        public string Label
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRChartAxisGetLabel(handle));
            }
            set
            {
                RawAPI.GRChartAxisSetLabel(handle, value);
            }
        }
        public Pen LinePen
        {
            get
            {
                IntPtr theHandle = RawAPI.GRChartAxisGetLinePen(handle);
                return theHandle != IntPtr.Zero ? new Pen(theHandle) : null;
            }
        }
        public bool LineVisible
        {
            get
            {
                return RawAPI.GRChartAxisGetLineVisible(handle);
            }
            set
            {
                RawAPI.GRChartAxisSetLineVisible(handle, value);
            }
        }
        public int MarginBeginWeight
        {
            get
            {
                return RawAPI.GRChartAxisGetMarginBeginWeight(handle);
            }
            set
            {
                RawAPI.GRChartAxisSetMarginBeginWeight(handle, value);
            }
        }
        public int MarginEndWeight
        {
            get
            {
                return RawAPI.GRChartAxisGetMarginEndWeight(handle);
            }
            set
            {
                RawAPI.GRChartAxisSetMarginEndWeight(handle, value);
            }
        }
        public double Max
        {
            get
            {
                return RawAPI.GRChartAxisGetMax(handle);
            }
            set
            {
                RawAPI.GRChartAxisSetMax(handle, value);
            }
        }
        public bool MaxAtScale
        {
            get
            {
                return RawAPI.GRChartAxisGetMaxAtScale(handle);
            }
            set
            {
                RawAPI.GRChartAxisSetMaxAtScale(handle, value);
            }
        }
        public double Min
        {
            get
            {
                return RawAPI.GRChartAxisGetMin(handle);
            }
            set
            {
                RawAPI.GRChartAxisSetMin(handle, value);
            }
        }
        public int ScaleCount
        {
            get
            {
                return RawAPI.GRChartAxisGetScaleCount(handle);
            }
        }
        public double Space
        {
            get
            {
                return RawAPI.GRChartAxisGetSpace(handle);
            }
            set
            {
                RawAPI.GRChartAxisSetSpace(handle, value);
            }
        }
        public double TextAngle
        {
            get
            {
                return RawAPI.GRChartAxisGetTextAngle(handle);
            }
            set
            {
                RawAPI.GRChartAxisSetTextAngle(handle, value);
            }
        }
        public string TextFormat
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRChartAxisGetTextFormat(handle));
            }
            set
            {
                RawAPI.GRChartAxisSetTextFormat(handle, value);
            }
        }
        public bool TextVisible
        {
            get
            {
                return RawAPI.GRChartAxisGetTextVisible(handle);
            }
            set
            {
                RawAPI.GRChartAxisSetTextVisible(handle, value);
            }
        }

        public void AddCustomCoordLine(double PosVal, string Text, double LineWidth, uint LineColor, PenStyle LineStyle)
        {
            RawAPI.GRChartAxisAddCustomCoordLine(handle, PosVal, Text, LineWidth, LineColor, LineStyle);
        }
        public void AddCustomScale(double ScaleValue, string ScaleText)
        {
            RawAPI.GRChartAxisAddCustomScale(handle, ScaleValue, ScaleText);
        }
        public void EmptyCustomCoordLine()
        {
            RawAPI.GRChartAxisEmptyCustomCoordLine(handle);
        }
        public void EmptyCustomScale()
        {
            RawAPI.GRChartAxisEmptyCustomScale(handle);
        }
        public string GetScaleText(int Index)
        {
            return Marshal.PtrToStringUni(RawAPI.GRChartAxisGetScaleText(handle, Index));
        }
        public double GetScaleValue(int Index)
        {
            return RawAPI.GRChartAxisGetScaleValue(handle, Index);
        }
    }

    public class ChartSeries : Object
    {
        public ChartSeries(IntPtr _handle)
            : base(_handle)
        {
        }
        public Pen BorderPen
        {
            get
            {
                IntPtr theHandle = RawAPI.GRChartSeriesGetBorderPen(handle);
                return theHandle != IntPtr.Zero ? new Pen(theHandle) : null;
            }
        }
        public bool ByY2Axis
        {
            get
            {
                return RawAPI.GRChartSeriesGetByY2Axis(handle);
            }
            set
            {
                RawAPI.GRChartSeriesSetByY2Axis(handle, value);
            }
        }
        public ChartType ChartType
        {
            get
            {
                return RawAPI.GRChartSeriesGetChartType(handle);
            }
            set
            {
                RawAPI.GRChartSeriesSetChartType(handle, value);
            }
        }
        public uint FillColor
        {
            get
            {
                return RawAPI.GRChartSeriesGetFillColor(handle);
            }
            set
            {
                RawAPI.GRChartSeriesSetFillColor(handle, value);
            }
        }
        public bool FillColorAuto
        {
            get
            {
                return RawAPI.GRChartSeriesGetFillColorAuto(handle);
            }
            set
            {
                RawAPI.GRChartSeriesSetFillColorAuto(handle, value);
            }
        }
        public bool LabelAsGroup
        {
            get
            {
                return RawAPI.GRChartSeriesGetLabelAsGroup(handle);
            }
            set
            {
                RawAPI.GRChartSeriesSetLabelAsGroup(handle, value);
            }
        }
        public bool LabelInBar
        {
            get
            {
                return RawAPI.GRChartSeriesGetLabelInBar(handle);
            }
            set
            {
                RawAPI.GRChartSeriesSetLabelInBar(handle, value);
            }
        }
        public string LabelText
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRChartSeriesGetLabelText(handle));
            }
            set
            {
                RawAPI.GRChartSeriesSetLabelText(handle, value);
            }
        }
        public double LabelTextAngle
        {
            get
            {
                return RawAPI.GRChartSeriesGetLabelTextAngle(handle);
            }
            set
            {
                RawAPI.GRChartSeriesSetLabelTextAngle(handle, value);
            }
        }
        public uint MarkerColor
        {
            get
            {
                return RawAPI.GRChartSeriesGetMarkerColor(handle);
            }
            set
            {
                RawAPI.GRChartSeriesSetMarkerColor(handle, value);
            }
        }
        public bool MarkerColorAuto
        {
            get
            {
                return RawAPI.GRChartSeriesGetMarkerColorAuto(handle);
            }
            set
            {
                RawAPI.GRChartSeriesSetMarkerColorAuto(handle, value);
            }
        }
        public Pen MarkerPen
        {
            get
            {
                IntPtr theHandle = RawAPI.GRChartSeriesGetMarkerPen(handle);
                return theHandle != IntPtr.Zero ? new Pen(theHandle) : null;
            }
        }
        public int MarkerSize
        {
            get
            {
                return RawAPI.GRChartSeriesGetMarkerSize(handle);
            }
            set
            {
                RawAPI.GRChartSeriesSetMarkerSize(handle, value);
            }
        }
        public PointMarkerStyle MarkerStyle
        {
            get
            {
                return RawAPI.GRChartSeriesGetMarkerStyle(handle);
            }
            set
            {
                RawAPI.GRChartSeriesSetMarkerStyle(handle, value);
            }
        }
        public string TooltipText
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRChartSeriesGetTooltipText(handle));
            }
            set
            {
                RawAPI.GRChartSeriesSetTooltipText(handle, value);
            }
        }
        public string ValueFormat
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRChartSeriesGetValueFormat(handle));
            }
            set
            {
                RawAPI.GRChartSeriesSetValueFormat(handle, value);
            }
        }
        public string XValueField
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRChartSeriesGetXValueField(handle));
            }
            set
            {
                RawAPI.GRChartSeriesSetXValueField(handle, value);
            }
        }
        public string YValueField
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRChartSeriesGetYValueField(handle));
            }
            set
            {
                RawAPI.GRChartSeriesSetYValueField(handle, value);
            }
        }
        public string ZValueField
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRChartSeriesGetZValueField(handle));
            }
            set
            {
                RawAPI.GRChartSeriesSetZValueField(handle, value);
            }
        }

    }

    public class Chart : Control
    {
        public Chart(IntPtr _handle)
            : base(_handle)
        {
        }
        public int BarWidthPercent
        {
            get
            {
                return RawAPI.GRChartGetBarWidthPercent(handle);
            }
            set
            {
                RawAPI.GRChartSetBarWidthPercent(handle, value);
            }
        }
        public double BubbleScalePerCm
        {
            get
            {
                return RawAPI.GRChartGetBubbleScalePerCm(handle);
            }
            set
            {
                RawAPI.GRChartSetBubbleScalePerCm(handle, value);
            }
        }
        public bool Chart3D
        {
            get
            {
                return RawAPI.GRChartGetChart3D(handle);
            }
            set
            {
                RawAPI.GRChartSetChart3D(handle, value);
            }
        }
        public int FillColorCount
        {
            get
            {
                return RawAPI.GRChartGetFillColorCount(handle);
            }
        }
        public bool GroupAuto
        {
            get
            {
                return RawAPI.GRChartGetGroupAuto(handle);
            }
            set
            {
                RawAPI.GRChartSetGroupAuto(handle, value);
            }
        }
        public int GroupCount
        {
            get
            {
                return RawAPI.GRChartGetGroupCount(handle);
            }
            set
            {
                RawAPI.GRChartSetGroupCount(handle, value);
            }
        }
        public string GroupField
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRChartGetGroupField(handle));
            }
            set
            {
                RawAPI.GRChartSetGroupField(handle, value);
            }
        }
        public bool LegendAtBottom
        {
            get
            {
                return RawAPI.GRChartGetLegendAtBottom(handle);
            }
            set
            {
                RawAPI.GRChartSetLegendAtBottom(handle, value);
            }
        }
        public Pen LegendBorderPen
        {
            get
            {
                IntPtr theHandle = RawAPI.GRChartGetLegendBorderPen(handle);
                return theHandle != IntPtr.Zero ? new Pen(theHandle) : null;
            }
        }
        public int LegendColumnCount
        {
            get
            {
                return RawAPI.GRChartGetLegendColumnCount(handle);
            }
            set
            {
                RawAPI.GRChartSetLegendColumnCount(handle, value);
            }
        }
        public DisplayCursor LegendCursor
        {
            get
            {
                return RawAPI.GRChartGetLegendCursor(handle);
            }
            set
            {
                RawAPI.GRChartSetLegendCursor(handle, value);
            }
        }
        public bool LegendShowSum
        {
            get
            {
                return RawAPI.GRChartGetLegendShowSum(handle);
            }
            set
            {
                RawAPI.GRChartSetLegendShowSum(handle, value);
            }
        }
        public string LegendSumLabel
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRChartGetLegendSumLabel(handle));
            }
            set
            {
                RawAPI.GRChartSetLegendSumLabel(handle, value);
            }
        }
        public bool LegendValueVisible
        {
            get
            {
                return RawAPI.GRChartGetLegendValueVisible(handle);
            }
            set
            {
                RawAPI.GRChartSetLegendValueVisible(handle, value);
            }
        }
        public bool LegendVisible
        {
            get
            {
                return RawAPI.GRChartGetLegendVisible(handle);
            }
            set
            {
                RawAPI.GRChartSetLegendVisible(handle, value);
            }
        }
        public Recordset Recordset
        {
            get
            {
                IntPtr theHandle = RawAPI.GRChartGetRecordset(handle);
                return theHandle != IntPtr.Zero ? new Recordset(theHandle) : null;
            }
        }
        public ChartSeriess Series
        {
            get
            {
                IntPtr theHandle = RawAPI.GRChartGetSeries(handle);
                return theHandle != IntPtr.Zero ? new ChartSeriess(theHandle) : null;
            }
        }
        public bool SeriesAuto
        {
            get
            {
                return RawAPI.GRChartGetSeriesAuto(handle);
            }
            set
            {
                RawAPI.GRChartSetSeriesAuto(handle, value);
            }
        }
        public int SeriesCount
        {
            get
            {
                return RawAPI.GRChartGetSeriesCount(handle);
            }
            set
            {
                RawAPI.GRChartSetSeriesCount(handle, value);
            }
        }
        public DisplayCursor SeriesCursor
        {
            get
            {
                return RawAPI.GRChartGetSeriesCursor(handle);
            }
            set
            {
                RawAPI.GRChartSetSeriesCursor(handle, value);
            }
        }
        public string SeriesField
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRChartGetSeriesField(handle));
            }
            set
            {
                RawAPI.GRChartSetSeriesField(handle, value);
            }
        }
        public bool SingleSeriesColored
        {
            get
            {
                return RawAPI.GRChartGetSingleSeriesColored(handle);
            }
            set
            {
                RawAPI.GRChartSetSingleSeriesColored(handle, value);
            }
        }
        public string Title
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRChartGetTitle(handle));
            }
            set
            {
                RawAPI.GRChartSetTitle(handle, value);
            }
        }
        public Font TitleFont
        {
            get
            {
                IntPtr theHandle = RawAPI.GRChartGetTitleFont(handle);
                return theHandle != IntPtr.Zero ? new Font(theHandle) : null;
            }
        }
        public Font ValueFont
        {
            get
            {
                IntPtr theHandle = RawAPI.GRChartGetValueFont(handle);
                return theHandle != IntPtr.Zero ? new Font(theHandle) : null;
            }
        }
        public ChartAxis XAxis
        {
            get
            {
                IntPtr theHandle = RawAPI.GRChartGetXAxis(handle);
                return theHandle != IntPtr.Zero ? new ChartAxis(theHandle) : null;
            }
        }
        public ChartAxis Y2Axis
        {
            get
            {
                IntPtr theHandle = RawAPI.GRChartGetY2Axis(handle);
                return theHandle != IntPtr.Zero ? new ChartAxis(theHandle) : null;
            }
        }
        public ChartAxis YAxis
        {
            get
            {
                IntPtr theHandle = RawAPI.GRChartGetYAxis(handle);
                return theHandle != IntPtr.Zero ? new ChartAxis(theHandle) : null;
            }
        }

        public void AddFillColor(uint FillColor)
        {
            RawAPI.GRChartAddFillColor(handle, FillColor);
        }
        public void AddXYValue(int SeriesIndex, double XVal, double YVal)
        {
            RawAPI.GRChartAddXYValue(handle, SeriesIndex, XVal, YVal);
        }
        public void AddXYZValue(int SeriesIndex, double XVal, double YVal, double ZVal)
        {
            RawAPI.GRChartAddXYZValue(handle, SeriesIndex, XVal, YVal, ZVal);
        }
        public void EmptyFillColors()
        {
            RawAPI.GRChartEmptyFillColors(handle);
        }
        public void EmptyValues()
        {
            RawAPI.GRChartEmptyValues(handle);
        }
        public uint FillColor(int Index)
        {
            return RawAPI.GRChartFillColor(handle, Index);
        }
        public string GroupLabel(int GroupIndex)
        {
            return Marshal.PtrToStringUni(RawAPI.GRChartGroupLabel(handle, GroupIndex));
        }
        public bool LoadDataFromXML(string XML, bool FirstSeries, bool AutoSeries, bool AutoGroup)
        {
            return RawAPI.GRChartLoadDataFromXML(handle, XML, FirstSeries, AutoSeries, AutoGroup);
        }
        public bool LoadXYDataFromXML(string XML, bool AutoSeries)
        {
            return RawAPI.GRChartLoadXYDataFromXML(handle, XML, AutoSeries);
        }
        public bool LoadXYZDataFromXML(string XML, bool AutoSeries)
        {
            return RawAPI.GRChartLoadXYZDataFromXML(handle, XML, AutoSeries);
        }
        public string SeriesLabel(int SeriesIndex)
        {
            return Marshal.PtrToStringUni(RawAPI.GRChartSeriesLabel(handle, SeriesIndex));
        }
        public void SetGroupLabel(int GroupIndex, string Text)
        {
            RawAPI.GRChartSetGroupLabel(handle, GroupIndex, Text);
        }
        public void SetSeriesLabel(int Index, string Text)
        {
            RawAPI.GRChartSetSeriesLabel(handle, Index, Text);
        }
        public void SetValue(int SeriesIndex, int GroupIndex, double Value)
        {
            RawAPI.GRChartSetValue(handle, SeriesIndex, GroupIndex, Value);
        }
        public double Value(int SeriesIndex, int GroupIndex)
        {
            return RawAPI.GRChartValue(handle, SeriesIndex, GroupIndex);
        }
        public double XValue(int SeriesIndex, int Index)
        {
            return RawAPI.GRChartXValue(handle, SeriesIndex, Index);
        }
        public int XYZValueCount(int SeriesIndex)
        {
            return RawAPI.GRChartXYZValueCount(handle, SeriesIndex);
        }
        public double YValue(int SeriesIndex, int Index)
        {
            return RawAPI.GRChartYValue(handle, SeriesIndex, Index);
        }
        public double ZValue(int SeriesIndex, int Index)
        {
            return RawAPI.GRChartZValue(handle, SeriesIndex, Index);
        }
    }

    public class FreeGridCell : CellBase
    {
        public FreeGridCell(IntPtr _handle)
            : base(_handle)
        {
        }
        public int ColSpan
        {
            get
            {
                return RawAPI.GRFreeGridCellGetColSpan(handle);
            }
            set
            {
                RawAPI.GRFreeGridCellSetColSpan(handle, value);
            }
        }
        public int ColumnIndex
        {
            get
            {
                return RawAPI.GRFreeGridCellGetColumnIndex(handle);
            }
        }
        public string DataName
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRFreeGridCellGetDataName(handle));
            }
            set
            {
                RawAPI.GRFreeGridCellSetDataName(handle, value);
            }
        }
        public FreeGrid FreeGrid
        {
            get
            {
                IntPtr theHandle = RawAPI.GRFreeGridCellGetFreeGrid(handle);
                return theHandle != IntPtr.Zero ? new FreeGrid(theHandle) : null;
            }
        }
        public int RowIndex
        {
            get
            {
                return RawAPI.GRFreeGridCellGetRowIndex(handle);
            }
        }
        public int RowSpan
        {
            get
            {
                return RawAPI.GRFreeGridCellGetRowSpan(handle);
            }
            set
            {
                RawAPI.GRFreeGridCellSetRowSpan(handle, value);
            }
        }
        public string Text
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRFreeGridCellGetText(handle));
            }
            set
            {
                RawAPI.GRFreeGridCellSetText(handle, value);
            }
        }

    }

    public class FreeGridColumn : Object
    {
        public FreeGridColumn(IntPtr _handle)
            : base(_handle)
        {
        }
        public int ColumnIndex
        {
            get
            {
                return RawAPI.GRFreeGridColumnGetColumnIndex(handle);
            }
        }
        public FreeGrid FreeGrid
        {
            get
            {
                IntPtr theHandle = RawAPI.GRFreeGridColumnGetFreeGrid(handle);
                return theHandle != IntPtr.Zero ? new FreeGrid(theHandle) : null;
            }
        }
        public bool Visible
        {
            get
            {
                return RawAPI.GRFreeGridColumnGetVisible(handle);
            }
            set
            {
                RawAPI.GRFreeGridColumnSetVisible(handle, value);
            }
        }
        public double Width
        {
            get
            {
                return RawAPI.GRFreeGridColumnGetWidth(handle);
            }
            set
            {
                RawAPI.GRFreeGridColumnSetWidth(handle, value);
            }
        }

    }

    public class FreeGridRow : Section
    {
        public FreeGridRow(IntPtr _handle)
            : base(_handle)
        {
        }
        public FreeGrid FreeGrid
        {
            get
            {
                IntPtr theHandle = RawAPI.GRFreeGridRowGetFreeGrid(handle);
                return theHandle != IntPtr.Zero ? new FreeGrid(theHandle) : null;
            }
        }
        public int RowIndex
        {
            get
            {
                return RawAPI.GRFreeGridRowGetRowIndex(handle);
            }
        }

    }

    public class FreeGrid : Control
    {
        public FreeGrid(IntPtr _handle)
            : base(_handle)
        {
        }
        public Pen ColLinePen
        {
            get
            {
                IntPtr theHandle = RawAPI.GRFreeGridGetColLinePen(handle);
                return theHandle != IntPtr.Zero ? new Pen(theHandle) : null;
            }
        }
        public int ColumnCount
        {
            get
            {
                return RawAPI.GRFreeGridGetColumnCount(handle);
            }
            set
            {
                RawAPI.GRFreeGridSetColumnCount(handle, value);
            }
        }
        public double DesignHeight
        {
            get
            {
                return RawAPI.GRFreeGridGetDesignHeight(handle);
            }
        }
        public double DesignWidth
        {
            get
            {
                return RawAPI.GRFreeGridGetDesignWidth(handle);
            }
        }
        public bool GrowToBottom
        {
            get
            {
                return RawAPI.GRFreeGridGetGrowToBottom(handle);
            }
            set
            {
                RawAPI.GRFreeGridSetGrowToBottom(handle, value);
            }
        }
        public int RowCount
        {
            get
            {
                return RawAPI.GRFreeGridGetRowCount(handle);
            }
            set
            {
                RawAPI.GRFreeGridSetRowCount(handle, value);
            }
        }
        public Pen RowLinePen
        {
            get
            {
                IntPtr theHandle = RawAPI.GRFreeGridGetRowLinePen(handle);
                return theHandle != IntPtr.Zero ? new Pen(theHandle) : null;
            }
        }
        public bool ShowColLine
        {
            get
            {
                return RawAPI.GRFreeGridGetShowColLine(handle);
            }
            set
            {
                RawAPI.GRFreeGridSetShowColLine(handle, value);
            }
        }
        public bool ShowRowLine
        {
            get
            {
                return RawAPI.GRFreeGridGetShowRowLine(handle);
            }
            set
            {
                RawAPI.GRFreeGridSetShowRowLine(handle, value);
            }
        }
        public bool TitleRepeat
        {
            get
            {
                return RawAPI.GRFreeGridGetTitleRepeat(handle);
            }
            set
            {
                RawAPI.GRFreeGridSetTitleRepeat(handle, value);
            }
        }
        public int TitleRows
        {
            get
            {
                return RawAPI.GRFreeGridGetTitleRows(handle);
            }
            set
            {
                RawAPI.GRFreeGridSetTitleRows(handle, value);
            }
        }

        public FreeGridCell CellAt(int RowIndex, int ColumnIndex)
        {
            IntPtr theHandle = RawAPI.GRFreeGridCellAt(handle, RowIndex, ColumnIndex);
            return theHandle != IntPtr.Zero ? new FreeGridCell(theHandle) : null;
        }
        public FreeGridCell CellByDataName(string DataName)
        {
            IntPtr theHandle = RawAPI.GRFreeGridCellByDataName(handle, DataName);
            return theHandle != IntPtr.Zero ? new FreeGridCell(theHandle) : null;
        }
        public FreeGridColumn ColumnAt(int ColumnIndex)
        {
            IntPtr theHandle = RawAPI.GRFreeGridColumnAt(handle, ColumnIndex);
            return theHandle != IntPtr.Zero ? new FreeGridColumn(theHandle) : null;
        }
        public FreeGridRow RowAt(int RowIndex)
        {
            IntPtr theHandle = RawAPI.GRFreeGridRowAt(handle, RowIndex);
            return theHandle != IntPtr.Zero ? new FreeGridRow(theHandle) : null;
        }
    }

    public class Field : Object
    {
        public Field(IntPtr _handle)
            : base(_handle)
        {
        }
        public bool AsBoolean
        {
            get
            {
                return RawAPI.GRFieldGetAsBoolean(handle);
            }
            set
            {
                RawAPI.GRFieldSetAsBoolean(handle, value);
            }
        }
        public long AsCurrency
        {
            get
            {
                return RawAPI.GRFieldGetAsCurrency(handle);
            }
            set
            {
                RawAPI.GRFieldSetAsCurrency(handle, value);
            }
        }
        public long AsDateTime
        {
            get
            {
                return RawAPI.GRFieldGetAsDateTime(handle);
            }
            set
            {
                RawAPI.GRFieldSetAsDateTime(handle, value);
            }
        }
        public double AsFloat
        {
            get
            {
                return RawAPI.GRFieldGetAsFloat(handle);
            }
            set
            {
                RawAPI.GRFieldSetAsFloat(handle, value);
            }
        }
        public int AsInteger
        {
            get
            {
                return RawAPI.GRFieldGetAsInteger(handle);
            }
            set
            {
                RawAPI.GRFieldSetAsInteger(handle, value);
            }
        }
        public string AsString
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRFieldGetAsString(handle));
            }
            set
            {
                RawAPI.GRFieldSetAsString(handle, value);
            }
        }
        public IntPtr DataBuffer
        {
            get
            {
                return RawAPI.GRFieldGetDataBuffer(handle);
            }
        }
        public int DataSize
        {
            get
            {
                return RawAPI.GRFieldGetDataSize(handle);
            }
        }
        public string DBFieldName
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRFieldGetDBFieldName(handle));
            }
            set
            {
                RawAPI.GRFieldSetDBFieldName(handle, value);
            }
        }
        public string DisplayText
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRFieldGetDisplayText(handle));
            }
            set
            {
                RawAPI.GRFieldSetDisplayText(handle, value);
            }
        }
        public FieldType FieldType
        {
            get
            {
                return RawAPI.GRFieldGetFieldType(handle);
            }
            set
            {
                RawAPI.GRFieldSetFieldType(handle, value);
            }
        }
        public string Format
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRFieldGetFormat(handle));
            }
            set
            {
                RawAPI.GRFieldSetFormat(handle, value);
            }
        }
        public string GetDisplayTextScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRFieldGetGetDisplayTextScript(handle));
            }
            set
            {
                RawAPI.GRFieldSetGetDisplayTextScript(handle, value);
            }
        }
        public bool IsNull
        {
            get
            {
                return RawAPI.GRFieldGetIsNull(handle);
            }
        }
        public string Name
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRFieldGetName(handle));
            }
            set
            {
                RawAPI.GRFieldSetName(handle, value);
            }
        }
        public bool RTrimBlankChars
        {
            get
            {
                return RawAPI.GRFieldGetRTrimBlankChars(handle);
            }
            set
            {
                RawAPI.GRFieldSetRTrimBlankChars(handle, value);
            }
        }
        public string Value
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRFieldGetValue(handle));
            }
            set
            {
                RawAPI.GRFieldSetValue(handle, value);
            }
        }

        public void Clear()
        {
            RawAPI.GRFieldClear(handle);
        }
        public void LoadFromBinary(BinaryObject BinaryObject)
        {
            RawAPI.GRFieldLoadFromBinary(handle, BinaryObject.NativeHandle);
        }
        public void LoadFromFile(string PathFile)
        {
            RawAPI.GRFieldLoadFromFile(handle, PathFile);
        }
        public void LoadFromMemory(IntPtr pBuffer, int ByteCount)
        {
            RawAPI.GRFieldLoadFromMemory(handle, pBuffer, ByteCount);
        }
    }

    public class Recordset : Object
    {
        public Recordset(IntPtr _handle)
            : base(_handle)
        {
        }
        public string BeforePostRecordScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRRecordsetGetBeforePostRecordScript(handle));
            }
            set
            {
                RawAPI.GRRecordsetSetBeforePostRecordScript(handle, value);
            }
        }
        public string ConnectionString
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRRecordsetGetConnectionString(handle));
            }
            set
            {
                RawAPI.GRRecordsetSetConnectionString(handle, value);
            }
        }
        public bool Editable
        {
            get
            {
                return RawAPI.GRRecordsetGetEditable(handle);
            }
            set
            {
                RawAPI.GRRecordsetSetEditable(handle, value);
            }
        }
        public string FetchRecordScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRRecordsetGetFetchRecordScript(handle));
            }
            set
            {
                RawAPI.GRRecordsetSetFetchRecordScript(handle, value);
            }
        }
        public Fields Fields
        {
            get
            {
                IntPtr theHandle = RawAPI.GRRecordsetGetFields(handle);
                return theHandle != IntPtr.Zero ? new Fields(theHandle) : null;
            }
        }
        public string PageProcessRecordScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRRecordsetGetPageProcessRecordScript(handle));
            }
            set
            {
                RawAPI.GRRecordsetSetPageProcessRecordScript(handle, value);
            }
        }
        public string ProcessRecordScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRRecordsetGetProcessRecordScript(handle));
            }
            set
            {
                RawAPI.GRRecordsetSetProcessRecordScript(handle, value);
            }
        }
        public string QuerySQL
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRRecordsetGetQuerySQL(handle));
            }
            set
            {
                RawAPI.GRRecordsetSetQuerySQL(handle, value);
            }
        }
        public int RecordCount
        {
            get
            {
                return RawAPI.GRRecordsetGetRecordCount(handle);
            }
        }
        public int RecordNo
        {
            get
            {
                return RawAPI.GRRecordsetGetRecordNo(handle);
            }
        }
        public bool SkipQuery
        {
            get
            {
                return RawAPI.GRRecordsetGetSkipQuery(handle);
            }
            set
            {
                RawAPI.GRRecordsetSetSkipQuery(handle, value);
            }
        }
        public bool SortAsc
        {
            get
            {
                return RawAPI.GRRecordsetGetSortAsc(handle);
            }
            set
            {
                RawAPI.GRRecordsetSetSortAsc(handle, value);
            }
        }
        public bool SortCaseSensitive
        {
            get
            {
                return RawAPI.GRRecordsetGetSortCaseSensitive(handle);
            }
            set
            {
                RawAPI.GRRecordsetSetSortCaseSensitive(handle, value);
            }
        }
        public string SortFields
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRRecordsetGetSortFields(handle));
            }
            set
            {
                RawAPI.GRRecordsetSetSortFields(handle, value);
            }
        }
        public string XmlTableName
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRRecordsetGetXmlTableName(handle));
            }
            set
            {
                RawAPI.GRRecordsetSetXmlTableName(handle, value);
            }
        }

        public Field AddField(string Name, FieldType Type)
        {
            IntPtr theHandle = RawAPI.GRRecordsetAddField(handle, Name, Type);
            return theHandle != IntPtr.Zero ? new Field(theHandle) : null;
        }
        public bool Append()
        {
            return RawAPI.GRRecordsetAppend(handle);
        }
        public bool Bof()
        {
            return RawAPI.GRRecordsetBof(handle);
        }
        public void Cancel()
        {
            RawAPI.GRRecordsetCancel(handle);
        }
        public bool Duplicate()
        {
            return RawAPI.GRRecordsetDuplicate(handle);
        }
        public bool Edit()
        {
            return RawAPI.GRRecordsetEdit(handle);
        }
        public void Empty()
        {
            RawAPI.GRRecordsetEmpty(handle);
        }
        public bool Eof()
        {
            return RawAPI.GRRecordsetEof(handle);
        }
        public void FilterBegin()
        {
            RawAPI.GRRecordsetFilterBegin(handle);
        }
        public void FilterByCount(int MaxCount)
        {
            RawAPI.GRRecordsetFilterByCount(handle, MaxCount);
        }
        public void FilterByStep(int Step, bool AlwaysLast)
        {
            RawAPI.GRRecordsetFilterByStep(handle, Step, AlwaysLast);
        }
        public void FilterEnd()
        {
            RawAPI.GRRecordsetFilterEnd(handle);
        }
        public void FilterReset()
        {
            RawAPI.GRRecordsetFilterReset(handle);
        }
        public void FilterSelect()
        {
            RawAPI.GRRecordsetFilterSelect(handle);
        }
        public void First()
        {
            RawAPI.GRRecordsetFirst(handle);
        }
        public void Free()
        {
            RawAPI.GRRecordsetFree(handle);
            _Free();
        }
        public void Last()
        {
            RawAPI.GRRecordsetLast(handle);
        }
        public bool LoadDataFromXML(string DataText)
        {
            return RawAPI.GRRecordsetLoadDataFromXML(handle, DataText);
        }
        public void MoveBy(int Delta)
        {
            RawAPI.GRRecordsetMoveBy(handle, Delta);
        }
        public void MoveTo(int RecordNo)
        {
            RawAPI.GRRecordsetMoveTo(handle, RecordNo);
        }
        public void Next()
        {
            RawAPI.GRRecordsetNext(handle);
        }
        public bool Post()
        {
            return RawAPI.GRRecordsetPost(handle);
        }
        public void Prior()
        {
            RawAPI.GRRecordsetPrior(handle);
        }
        public void Resort(string Fields, bool Ascending, bool CaseSensitive)
        {
            RawAPI.GRRecordsetResort(handle, Fields, Ascending, CaseSensitive);
        }
        public string SaveDataToXML()
        {
            return Marshal.PtrToStringUni(RawAPI.GRRecordsetSaveDataToXML(handle));
        }
    }

    public class Column : Object
    {
        public Column(IntPtr _handle)
            : base(_handle)
        {
        }
        public ColumnContentCell ContentCell
        {
            get
            {
                IntPtr theHandle = RawAPI.GRColumnGetContentCell(handle);
                return theHandle != IntPtr.Zero ? new ColumnContentCell(theHandle) : null;
            }
        }
        public DetailGrid DetailGrid
        {
            get
            {
                IntPtr theHandle = RawAPI.GRColumnGetDetailGrid(handle);
                return theHandle != IntPtr.Zero ? new DetailGrid(theHandle) : null;
            }
        }
        public bool FixedWidth
        {
            get
            {
                return RawAPI.GRColumnGetFixedWidth(handle);
            }
            set
            {
                RawAPI.GRColumnSetFixedWidth(handle, value);
            }
        }
        public string Name
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRColumnGetName(handle));
            }
            set
            {
                RawAPI.GRColumnSetName(handle, value);
            }
        }
        public ColumnTitleCell TitleCell
        {
            get
            {
                IntPtr theHandle = RawAPI.GRColumnGetTitleCell(handle);
                return theHandle != IntPtr.Zero ? new ColumnTitleCell(theHandle) : null;
            }
        }
        public bool Visible
        {
            get
            {
                return RawAPI.GRColumnGetVisible(handle);
            }
            set
            {
                RawAPI.GRColumnSetVisible(handle, value);
            }
        }
        public double Width
        {
            get
            {
                return RawAPI.GRColumnGetWidth(handle);
            }
            set
            {
                RawAPI.GRColumnSetWidth(handle, value);
            }
        }

    }

    public class ColumnCell : CellBase
    {
        public ColumnCell(IntPtr _handle)
            : base(_handle)
        {
        }
        public Column Column
        {
            get
            {
                IntPtr theHandle = RawAPI.GRColumnCellGetColumn(handle);
                return theHandle != IntPtr.Zero ? new Column(theHandle) : null;
            }
        }
        public string Name
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRColumnCellGetName(handle));
            }
            set
            {
                RawAPI.GRColumnCellSetName(handle, value);
            }
        }

    }

    public class ColumnContentCell : ColumnCell
    {
        public ColumnContentCell(IntPtr _handle)
            : base(_handle)
        {
        }
        public ColumnContent ColumnContent
        {
            get
            {
                IntPtr theHandle = RawAPI.GRColumnContentCellGetColumnContent(handle);
                return theHandle != IntPtr.Zero ? new ColumnContent(theHandle) : null;
            }
        }
        public string DataField
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRColumnContentCellGetDataField(handle));
            }
            set
            {
                RawAPI.GRColumnContentCellSetDataField(handle, value);
            }
        }

    }

    public class ColumnTitleCell : ColumnCell
    {
        public ColumnTitleCell(IntPtr _handle)
            : base(_handle)
        {
        }
        public ColumnTitle ColumnTitle
        {
            get
            {
                IntPtr theHandle = RawAPI.GRColumnTitleCellGetColumnTitle(handle);
                return theHandle != IntPtr.Zero ? new ColumnTitle(theHandle) : null;
            }
        }
        public bool GroupTitle
        {
            get
            {
                return RawAPI.GRColumnTitleCellGetGroupTitle(handle);
            }
        }
        public double Height
        {
            get
            {
                return RawAPI.GRColumnTitleCellGetHeight(handle);
            }
            set
            {
                RawAPI.GRColumnTitleCellSetHeight(handle, value);
            }
        }
        public ColumnTitleCells SubTitles
        {
            get
            {
                IntPtr theHandle = RawAPI.GRColumnTitleCellGetSubTitles(handle);
                return theHandle != IntPtr.Zero ? new ColumnTitleCells(theHandle) : null;
            }
        }
        public ColumnTitleCell SupCell
        {
            get
            {
                IntPtr theHandle = RawAPI.GRColumnTitleCellGetSupCell(handle);
                return theHandle != IntPtr.Zero ? new ColumnTitleCell(theHandle) : null;
            }
        }
        public string Text
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRColumnTitleCellGetText(handle));
            }
            set
            {
                RawAPI.GRColumnTitleCellSetText(handle, value);
            }
        }
        public bool Visible
        {
            get
            {
                return RawAPI.GRColumnTitleCellGetVisible(handle);
            }
            set
            {
                RawAPI.GRColumnTitleCellSetVisible(handle, value);
            }
        }

        public ColumnTitleCell AddSubGroupTitle(string Name, string Title)
        {
            IntPtr theHandle = RawAPI.GRColumnTitleCellAddSubGroupTitle(handle, Name, Title);
            return theHandle != IntPtr.Zero ? new ColumnTitleCell(theHandle) : null;
        }
        public void EncloseColumn(string ColumnName)
        {
            RawAPI.GRColumnTitleCellEncloseColumn(handle, ColumnName);
        }
    }

    public class ColumnSection : Section
    {
        public ColumnSection(IntPtr _handle)
            : base(_handle)
        {
        }
        public DetailGrid DetailGrid
        {
            get
            {
                IntPtr theHandle = RawAPI.GRColumnSectionGetDetailGrid(handle);
                return theHandle != IntPtr.Zero ? new DetailGrid(theHandle) : null;
            }
        }

        public void SetCellsBackColor(uint BackColor)
        {
            RawAPI.GRColumnSectionSetCellsBackColor(handle, BackColor);
        }
        public void SetCellsForeColor(uint ForeColor)
        {
            RawAPI.GRColumnSectionSetCellsForeColor(handle, ForeColor);
        }
    }

    public class ColumnContent : ColumnSection
    {
        public ColumnContent(IntPtr _handle)
            : base(_handle)
        {
        }
        public bool AdjustRowHeight
        {
            get
            {
                return RawAPI.GRColumnContentGetAdjustRowHeight(handle);
            }
            set
            {
                RawAPI.GRColumnContentSetAdjustRowHeight(handle, value);
            }
        }
        public uint AlternatingBackColor
        {
            get
            {
                return RawAPI.GRColumnContentGetAlternatingBackColor(handle);
            }
            set
            {
                RawAPI.GRColumnContentSetAlternatingBackColor(handle, value);
            }
        }
        public ColumnContentCells ContentCells
        {
            get
            {
                IntPtr theHandle = RawAPI.GRColumnContentGetContentCells(handle);
                return theHandle != IntPtr.Zero ? new ColumnContentCells(theHandle) : null;
            }
        }
        public bool GrowToNextRow
        {
            get
            {
                return RawAPI.GRColumnContentGetGrowToNextRow(handle);
            }
            set
            {
                RawAPI.GRColumnContentSetGrowToNextRow(handle, value);
            }
        }
        public bool RowsIncludeGroup
        {
            get
            {
                return RawAPI.GRColumnContentGetRowsIncludeGroup(handle);
            }
            set
            {
                RawAPI.GRColumnContentSetRowsIncludeGroup(handle, value);
            }
        }
        public int RowsPerPage
        {
            get
            {
                return RawAPI.GRColumnContentGetRowsPerPage(handle);
            }
            set
            {
                RawAPI.GRColumnContentSetRowsPerPage(handle, value);
            }
        }

    }

    public class ColumnTitle : ColumnSection
    {
        public ColumnTitle(IntPtr _handle)
            : base(_handle)
        {
        }
        public bool BeforeHeaders
        {
            get
            {
                return RawAPI.GRColumnTitleGetBeforeHeaders(handle);
            }
            set
            {
                RawAPI.GRColumnTitleSetBeforeHeaders(handle, value);
            }
        }
        public RepeatStyle RepeatStyle
        {
            get
            {
                return RawAPI.GRColumnTitleGetRepeatStyle(handle);
            }
            set
            {
                RawAPI.GRColumnTitleSetRepeatStyle(handle, value);
            }
        }
        public ColumnTitleCells TitleCells
        {
            get
            {
                IntPtr theHandle = RawAPI.GRColumnTitleGetTitleCells(handle);
                return theHandle != IntPtr.Zero ? new ColumnTitleCells(theHandle) : null;
            }
        }

    }

    public class GroupSection : Section
    {
        public GroupSection(IntPtr _handle)
            : base(_handle)
        {
        }
        public Group Group
        {
            get
            {
                IntPtr theHandle = RawAPI.GRGroupSectionGetGroup(handle);
                return theHandle != IntPtr.Zero ? new Group(theHandle) : null;
            }
        }
        public bool HNewPageFixed
        {
            get
            {
                return RawAPI.GRGroupSectionGetHNewPageFixed(handle);
            }
            set
            {
                RawAPI.GRGroupSectionSetHNewPageFixed(handle, value);
            }
        }
        public NewPageStyle NewPage
        {
            get
            {
                return RawAPI.GRGroupSectionGetNewPage(handle);
            }
            set
            {
                RawAPI.GRGroupSectionSetNewPage(handle, value);
            }
        }
        public NewPageColumnStyle NewPageColumn
        {
            get
            {
                return RawAPI.GRGroupSectionGetNewPageColumn(handle);
            }
            set
            {
                RawAPI.GRGroupSectionSetNewPageColumn(handle, value);
            }
        }
        public bool PrintGridBorder
        {
            get
            {
                return RawAPI.GRGroupSectionGetPrintGridBorder(handle);
            }
            set
            {
                RawAPI.GRGroupSectionSetPrintGridBorder(handle, value);
            }
        }
        public bool RepeatOnPage
        {
            get
            {
                return RawAPI.GRGroupSectionGetRepeatOnPage(handle);
            }
            set
            {
                RawAPI.GRGroupSectionSetRepeatOnPage(handle, value);
            }
        }

    }

    public class GroupHeader : GroupSection
    {
        public GroupHeader(IntPtr _handle)
            : base(_handle)
        {
        }
        public GrpKpTogetherStyle GroupKeepTogether
        {
            get
            {
                return RawAPI.GRGroupHeaderGetGroupKeepTogether(handle);
            }
            set
            {
                RawAPI.GRGroupHeaderSetGroupKeepTogether(handle, value);
            }
        }
        public bool IncludeFooter
        {
            get
            {
                return RawAPI.GRGroupHeaderGetIncludeFooter(handle);
            }
            set
            {
                RawAPI.GRGroupHeaderSetIncludeFooter(handle, value);
            }
        }
        public string OccupiedColumns
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRGroupHeaderGetOccupiedColumns(handle));
            }
            set
            {
                RawAPI.GRGroupHeaderSetOccupiedColumns(handle, value);
            }
        }
        public bool OccupyColumn
        {
            get
            {
                return RawAPI.GRGroupHeaderGetOccupyColumn(handle);
            }
            set
            {
                RawAPI.GRGroupHeaderSetOccupyColumn(handle, value);
            }
        }
        public bool SameAsColumn
        {
            get
            {
                return RawAPI.GRGroupHeaderGetSameAsColumn(handle);
            }
            set
            {
                RawAPI.GRGroupHeaderSetSameAsColumn(handle, value);
            }
        }
        public OCGroupHeaderVAlign VAlign
        {
            get
            {
                return RawAPI.GRGroupHeaderGetVAlign(handle);
            }
            set
            {
                RawAPI.GRGroupHeaderSetVAlign(handle, value);
            }
        }

    }

    public class GroupFooter : GroupSection
    {
        public GroupFooter(IntPtr _handle)
            : base(_handle)
        {
        }
        public bool AppendBlankRowExclude
        {
            get
            {
                return RawAPI.GRGroupFooterGetAppendBlankRowExclude(handle);
            }
            set
            {
                RawAPI.GRGroupFooterSetAppendBlankRowExclude(handle, value);
            }
        }
        public bool PrintAtBottom
        {
            get
            {
                return RawAPI.GRGroupFooterGetPrintAtBottom(handle);
            }
            set
            {
                RawAPI.GRGroupFooterSetPrintAtBottom(handle, value);
            }
        }

    }

    public class Group : Object
    {
        public Group(IntPtr _handle)
            : base(_handle)
        {
        }
        public string ByFields
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRGroupGetByFields(handle));
            }
            set
            {
                RawAPI.GRGroupSetByFields(handle, value);
            }
        }
        public DetailGrid DetailGrid
        {
            get
            {
                IntPtr theHandle = RawAPI.GRGroupGetDetailGrid(handle);
                return theHandle != IntPtr.Zero ? new DetailGrid(theHandle) : null;
            }
        }
        public GroupFooter Footer
        {
            get
            {
                IntPtr theHandle = RawAPI.GRGroupGetFooter(handle);
                return theHandle != IntPtr.Zero ? new GroupFooter(theHandle) : null;
            }
        }
        public string GroupBeginScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRGroupGetGroupBeginScript(handle));
            }
            set
            {
                RawAPI.GRGroupSetGroupBeginScript(handle, value);
            }
        }
        public string GroupEndScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRGroupGetGroupEndScript(handle));
            }
            set
            {
                RawAPI.GRGroupSetGroupEndScript(handle, value);
            }
        }
        public GroupHeader Header
        {
            get
            {
                IntPtr theHandle = RawAPI.GRGroupGetHeader(handle);
                return theHandle != IntPtr.Zero ? new GroupHeader(theHandle) : null;
            }
        }
        public int LimitsPerPage
        {
            get
            {
                return RawAPI.GRGroupGetLimitsPerPage(handle);
            }
            set
            {
                RawAPI.GRGroupSetLimitsPerPage(handle, value);
            }
        }
        public string Name
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRGroupGetName(handle));
            }
            set
            {
                RawAPI.GRGroupSetName(handle, value);
            }
        }
        public bool PageGroup
        {
            get
            {
                return RawAPI.GRGroupGetPageGroup(handle);
            }
            set
            {
                RawAPI.GRGroupSetPageGroup(handle, value);
            }
        }
        public bool SortAsc
        {
            get
            {
                return RawAPI.GRGroupGetSortAsc(handle);
            }
            set
            {
                RawAPI.GRGroupSetSortAsc(handle, value);
            }
        }
        public string SortSummaryBox
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRGroupGetSortSummaryBox(handle));
            }
            set
            {
                RawAPI.GRGroupSetSortSummaryBox(handle, value);
            }
        }

    }

    public class CrossTab : Object
    {
        public CrossTab(IntPtr _handle)
            : base(_handle)
        {
        }
        public string BeginDateParameter
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRCrossTabGetBeginDateParameter(handle));
            }
            set
            {
                RawAPI.GRCrossTabSetBeginDateParameter(handle, value);
            }
        }
        public string EndDateParameter
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRCrossTabGetEndDateParameter(handle));
            }
            set
            {
                RawAPI.GRCrossTabSetEndDateParameter(handle, value);
            }
        }
        public bool GroupAutoSum
        {
            get
            {
                return RawAPI.GRCrossTabGetGroupAutoSum(handle);
            }
            set
            {
                RawAPI.GRCrossTabSetGroupAutoSum(handle, value);
            }
        }
        public string HCrossFields
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRCrossTabGetHCrossFields(handle));
            }
            set
            {
                RawAPI.GRCrossTabSetHCrossFields(handle, value);
            }
        }
        public PeriodType HCrossPeriodType
        {
            get
            {
                return RawAPI.GRCrossTabGetHCrossPeriodType(handle);
            }
            set
            {
                RawAPI.GRCrossTabSetHCrossPeriodType(handle, value);
            }
        }
        public string HPercentColumns
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRCrossTabGetHPercentColumns(handle));
            }
            set
            {
                RawAPI.GRCrossTabSetHPercentColumns(handle, value);
            }
        }
        public bool HResort
        {
            get
            {
                return RawAPI.GRCrossTabGetHResort(handle);
            }
            set
            {
                RawAPI.GRCrossTabSetHResort(handle, value);
            }
        }
        public bool HSortAsc
        {
            get
            {
                return RawAPI.GRCrossTabGetHSortAsc(handle);
            }
            set
            {
                RawAPI.GRCrossTabSetHSortAsc(handle, value);
            }
        }
        public bool HTotalAtFirst
        {
            get
            {
                return RawAPI.GRCrossTabGetHTotalAtFirst(handle);
            }
            set
            {
                RawAPI.GRCrossTabSetHTotalAtFirst(handle, value);
            }
        }
        public int ListCols
        {
            get
            {
                return RawAPI.GRCrossTabGetListCols(handle);
            }
            set
            {
                RawAPI.GRCrossTabSetListCols(handle, value);
            }
        }
        public string PercentFormat
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRCrossTabGetPercentFormat(handle));
            }
            set
            {
                RawAPI.GRCrossTabSetPercentFormat(handle, value);
            }
        }
        public Recordset Recordset
        {
            get
            {
                IntPtr theHandle = RawAPI.GRCrossTabGetRecordset(handle);
                return theHandle != IntPtr.Zero ? new Recordset(theHandle) : null;
            }
        }
        public int SubtotalCols
        {
            get
            {
                return RawAPI.GRCrossTabGetSubtotalCols(handle);
            }
            set
            {
                RawAPI.GRCrossTabSetSubtotalCols(handle, value);
            }
        }
        public int TotalCols
        {
            get
            {
                return RawAPI.GRCrossTabGetTotalCols(handle);
            }
            set
            {
                RawAPI.GRCrossTabSetTotalCols(handle, value);
            }
        }
        public string TotalExcludeColumns
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRCrossTabGetTotalExcludeColumns(handle));
            }
            set
            {
                RawAPI.GRCrossTabSetTotalExcludeColumns(handle, value);
            }
        }
        public string TotalHPercentColumns
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRCrossTabGetTotalHPercentColumns(handle));
            }
            set
            {
                RawAPI.GRCrossTabSetTotalHPercentColumns(handle, value);
            }
        }
        public string TotalVPercentColumns
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRCrossTabGetTotalVPercentColumns(handle));
            }
            set
            {
                RawAPI.GRCrossTabSetTotalVPercentColumns(handle, value);
            }
        }
        public string VCrossFields
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRCrossTabGetVCrossFields(handle));
            }
            set
            {
                RawAPI.GRCrossTabSetVCrossFields(handle, value);
            }
        }
        public string VPercentColumns
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRCrossTabGetVPercentColumns(handle));
            }
            set
            {
                RawAPI.GRCrossTabSetVPercentColumns(handle, value);
            }
        }
        public bool VResort
        {
            get
            {
                return RawAPI.GRCrossTabGetVResort(handle);
            }
            set
            {
                RawAPI.GRCrossTabSetVResort(handle, value);
            }
        }
        public bool VSortAsc
        {
            get
            {
                return RawAPI.GRCrossTabGetVSortAsc(handle);
            }
            set
            {
                RawAPI.GRCrossTabSetVSortAsc(handle, value);
            }
        }

        public long GetCurPeriodBeginDate()
        {
            return RawAPI.GRCrossTabGetCurPeriodBeginDate(handle);
        }
        public long GetCurPeriodEndDate()
        {
            return RawAPI.GRCrossTabGetCurPeriodEndDate(handle);
        }
        public void HBeginAddItem()
        {
            RawAPI.GRCrossTabHBeginAddItem(handle);
        }
        public void HEndAddItem()
        {
            RawAPI.GRCrossTabHEndAddItem(handle);
        }
        public void VBeginAddItem()
        {
            RawAPI.GRCrossTabVBeginAddItem(handle);
        }
        public void VEndAddItem()
        {
            RawAPI.GRCrossTabVEndAddItem(handle);
        }
    }

    public class DetailGrid : Object
    {
        public DetailGrid(IntPtr _handle)
            : base(_handle)
        {
        }
        public bool AppendBlankCol
        {
            get
            {
                return RawAPI.GRDetailGridGetAppendBlankCol(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetAppendBlankCol(handle, value);
            }
        }
        public double AppendBlankColWidth
        {
            get
            {
                return RawAPI.GRDetailGridGetAppendBlankColWidth(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetAppendBlankColWidth(handle, value);
            }
        }
        public bool AppendBlankRow
        {
            get
            {
                return RawAPI.GRDetailGridGetAppendBlankRow(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetAppendBlankRow(handle, value);
            }
        }
        public bool AppendBlankRowAtLast
        {
            get
            {
                return RawAPI.GRDetailGridGetAppendBlankRowAtLast(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetAppendBlankRowAtLast(handle, value);
            }
        }
        public uint BackColor
        {
            get
            {
                return RawAPI.GRDetailGridGetBackColor(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetBackColor(handle, value);
            }
        }
        public Border Border
        {
            get
            {
                IntPtr theHandle = RawAPI.GRDetailGridGetBorder(handle);
                return theHandle != IntPtr.Zero ? new Border(theHandle) : null;
            }
        }
        public PrintType BorderPrintType
        {
            get
            {
                return RawAPI.GRDetailGridGetBorderPrintType(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetBorderPrintType(handle, value);
            }
        }
        public bool CenterView
        {
            get
            {
                return RawAPI.GRDetailGridGetCenterView(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetCenterView(handle, value);
            }
        }
        public Pen ColLinePen
        {
            get
            {
                IntPtr theHandle = RawAPI.GRDetailGridGetColLinePen(handle);
                return theHandle != IntPtr.Zero ? new Pen(theHandle) : null;
            }
        }
        public ColumnContent ColumnContent
        {
            get
            {
                IntPtr theHandle = RawAPI.GRDetailGridGetColumnContent(handle);
                return theHandle != IntPtr.Zero ? new ColumnContent(theHandle) : null;
            }
        }
        public Columns Columns
        {
            get
            {
                IntPtr theHandle = RawAPI.GRDetailGridGetColumns(handle);
                return theHandle != IntPtr.Zero ? new Columns(theHandle) : null;
            }
        }
        public ColumnTitle ColumnTitle
        {
            get
            {
                IntPtr theHandle = RawAPI.GRDetailGridGetColumnTitle(handle);
                return theHandle != IntPtr.Zero ? new ColumnTitle(theHandle) : null;
            }
        }
        public CrossTab CrossTab
        {
            get
            {
                IntPtr theHandle = RawAPI.GRDetailGridGetCrossTab(handle);
                return theHandle != IntPtr.Zero ? new CrossTab(theHandle) : null;
            }
        }
        public int FixCols
        {
            get
            {
                return RawAPI.GRDetailGridGetFixCols(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetFixCols(handle, value);
            }
        }
        public Font Font
        {
            get
            {
                IntPtr theHandle = RawAPI.GRDetailGridGetFont(handle);
                return theHandle != IntPtr.Zero ? new Font(theHandle) : null;
            }
        }
        public PrintType GridLinePrintType
        {
            get
            {
                return RawAPI.GRDetailGridGetGridLinePrintType(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetGridLinePrintType(handle, value);
            }
        }
        public Groups Groups
        {
            get
            {
                IntPtr theHandle = RawAPI.GRDetailGridGetGroups(handle);
                return theHandle != IntPtr.Zero ? new Groups(theHandle) : null;
            }
        }
        public bool GrowToBottom
        {
            get
            {
                return RawAPI.GRDetailGridGetGrowToBottom(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetGrowToBottom(handle, value);
            }
        }
        public bool IsCrossTab
        {
            get
            {
                return RawAPI.GRDetailGridGetIsCrossTab(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetIsCrossTab(handle, value);
            }
        }
        public NewPageStyle NewPage
        {
            get
            {
                return RawAPI.GRDetailGridGetNewPage(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetNewPage(handle, value);
            }
        }
        public int PageColumnCount
        {
            get
            {
                return RawAPI.GRDetailGridGetPageColumnCount(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetPageColumnCount(handle, value);
            }
        }
        public PageColumnDirection PageColumnDirection
        {
            get
            {
                return RawAPI.GRDetailGridGetPageColumnDirection(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetPageColumnDirection(handle, value);
            }
        }
        public bool PageColumnGroupNA
        {
            get
            {
                return RawAPI.GRDetailGridGetPageColumnGroupNA(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetPageColumnGroupNA(handle, value);
            }
        }
        public double PageColumnSpacing
        {
            get
            {
                return RawAPI.GRDetailGridGetPageColumnSpacing(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetPageColumnSpacing(handle, value);
            }
        }
        public bool PrintAdaptFitText
        {
            get
            {
                return RawAPI.GRDetailGridGetPrintAdaptFitText(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetPrintAdaptFitText(handle, value);
            }
        }
        public ColumnPrintAdaptMethod PrintAdaptMethod
        {
            get
            {
                return RawAPI.GRDetailGridGetPrintAdaptMethod(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetPrintAdaptMethod(handle, value);
            }
        }
        public bool PrintAdaptRepeat
        {
            get
            {
                return RawAPI.GRDetailGridGetPrintAdaptRepeat(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetPrintAdaptRepeat(handle, value);
            }
        }
        public int PrintAdaptRFCStep
        {
            get
            {
                return RawAPI.GRDetailGridGetPrintAdaptRFCStep(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetPrintAdaptRFCStep(handle, value);
            }
        }
        public bool PrintAdaptTryToOnePage
        {
            get
            {
                return RawAPI.GRDetailGridGetPrintAdaptTryToOnePage(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetPrintAdaptTryToOnePage(handle, value);
            }
        }
        public Recordset Recordset
        {
            get
            {
                IntPtr theHandle = RawAPI.GRDetailGridGetRecordset(handle);
                return theHandle != IntPtr.Zero ? new Recordset(theHandle) : null;
            }
        }
        public Pen RowLinePen
        {
            get
            {
                IntPtr theHandle = RawAPI.GRDetailGridGetRowLinePen(handle);
                return theHandle != IntPtr.Zero ? new Pen(theHandle) : null;
            }
        }
        public bool ShowColLine
        {
            get
            {
                return RawAPI.GRDetailGridGetShowColLine(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetShowColLine(handle, value);
            }
        }
        public bool ShowRowLine
        {
            get
            {
                return RawAPI.GRDetailGridGetShowRowLine(handle);
            }
            set
            {
                RawAPI.GRDetailGridSetShowRowLine(handle, value);
            }
        }

        public Column AddColumn(string Name, string Title, string DataField, double Width)
        {
            IntPtr theHandle = RawAPI.GRDetailGridAddColumn(handle, Name, Title, DataField, Width);
            return theHandle != IntPtr.Zero ? new Column(theHandle) : null;
        }
        public ColumnTitleCell AddGroupTitle(string Name, string Title)
        {
            IntPtr theHandle = RawAPI.GRDetailGridAddGroupTitle(handle, Name, Title);
            return theHandle != IntPtr.Zero ? new ColumnTitleCell(theHandle) : null;
        }
        public void ClearColumns()
        {
            RawAPI.GRDetailGridClearColumns(handle);
        }
        public void ClearGroupTitles()
        {
            RawAPI.GRDetailGridClearGroupTitles(handle);
        }
        public Column ColumnByShowOrder(int OrderNo)
        {
            IntPtr theHandle = RawAPI.GRDetailGridColumnByShowOrder(handle, OrderNo);
            return theHandle != IntPtr.Zero ? new Column(theHandle) : null;
        }
        public void ColumnMoveTo(string ColumnName, string NewGroupTitleCellName, int NewShowOrderNo)
        {
            RawAPI.GRDetailGridColumnMoveTo(handle, ColumnName, NewGroupTitleCellName, NewShowOrderNo);
        }
        public void ColumnMoveToEnd(string ColumnName)
        {
            RawAPI.GRDetailGridColumnMoveToEnd(handle, ColumnName);
        }
        public ColumnTitleCell FindGroupTitleCell(string Name)
        {
            IntPtr theHandle = RawAPI.GRDetailGridFindGroupTitleCell(handle, Name);
            return theHandle != IntPtr.Zero ? new ColumnTitleCell(theHandle) : null;
        }
        public void StartNewGroup(int Index)
        {
            RawAPI.GRDetailGridStartNewGroup(handle, Index);
        }
    }

    public class ReportSection : Section
    {
        public ReportSection(IntPtr _handle)
            : base(_handle)
        {
        }
        public bool CenterWithDetailGrid
        {
            get
            {
                return RawAPI.GRReportSectionGetCenterWithDetailGrid(handle);
            }
            set
            {
                RawAPI.GRReportSectionSetCenterWithDetailGrid(handle, value);
            }
        }
        public string Name
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportSectionGetName(handle));
            }
            set
            {
                RawAPI.GRReportSectionSetName(handle, value);
            }
        }
        public NewPageStyle NewPage
        {
            get
            {
                return RawAPI.GRReportSectionGetNewPage(handle);
            }
            set
            {
                RawAPI.GRReportSectionSetNewPage(handle, value);
            }
        }
        public bool RepeatOnPage
        {
            get
            {
                return RawAPI.GRReportSectionGetRepeatOnPage(handle);
            }
            set
            {
                RawAPI.GRReportSectionSetRepeatOnPage(handle, value);
            }
        }

    }

    public class PageHeader : Section
    {
        public PageHeader(IntPtr _handle)
            : base(_handle)
        {
        }

    }

    public class PageFooter : Section
    {
        public PageFooter(IntPtr _handle)
            : base(_handle)
        {
        }

    }

    public class ReportHeader : ReportSection
    {
        public ReportHeader(IntPtr _handle)
            : base(_handle)
        {
        }

    }

    public class ReportFooter : ReportSection
    {
        public ReportFooter(IntPtr _handle)
            : base(_handle)
        {
        }
        public bool AppendBlankRowExclude
        {
            get
            {
                return RawAPI.GRReportFooterGetAppendBlankRowExclude(handle);
            }
            set
            {
                RawAPI.GRReportFooterSetAppendBlankRowExclude(handle, value);
            }
        }
        public bool PrintAtBottom
        {
            get
            {
                return RawAPI.GRReportFooterGetPrintAtBottom(handle);
            }
            set
            {
                RawAPI.GRReportFooterSetPrintAtBottom(handle, value);
            }
        }

    }

    public class Controls : Base
    {
        public Controls(IntPtr _handle)
            : base(_handle)
        {
        }
        public int Count
        {
            get
            {
                return RawAPI.GRControlsGetCount(handle);
            }
        }

        public Control Add(ControlType Type)
        {
            IntPtr theHandle = RawAPI.GRControlsAdd(handle, Type);
            return theHandle != IntPtr.Zero ? new Control(theHandle) : null;
        }
        public void ChangeItemOrder(int Index, int NewOrder)
        {
            RawAPI.GRControlsChangeItemOrder(handle, Index, NewOrder);
        }
        public int IndexByName(string Name)
        {
            return RawAPI.GRControlsIndexByName(handle, Name);
        }
        public Control ItemAt(int Index)
        {
            IntPtr theHandle = RawAPI.GRControlsItemAt(handle, Index);
            return theHandle != IntPtr.Zero ? new Control(theHandle) : null;
        }
        public void Remove(int Index)
        {
            RawAPI.GRControlsRemove(handle, Index);
        }
        public void RemoveAll()
        {
            RawAPI.GRControlsRemoveAll(handle);
        }
    }

    public class Fields : Base
    {
        public Fields(IntPtr _handle)
            : base(_handle)
        {
        }
        public int Count
        {
            get
            {
                return RawAPI.GRFieldsGetCount(handle);
            }
        }

        public Field Add()
        {
            IntPtr theHandle = RawAPI.GRFieldsAdd(handle);
            return theHandle != IntPtr.Zero ? new Field(theHandle) : null;
        }
        public void ChangeItemOrder(int Index, int NewOrder)
        {
            RawAPI.GRFieldsChangeItemOrder(handle, Index, NewOrder);
        }
        public int IndexByName(string Name)
        {
            return RawAPI.GRFieldsIndexByName(handle, Name);
        }
        public Field ItemAt(int Index)
        {
            IntPtr theHandle = RawAPI.GRFieldsItemAt(handle, Index);
            return theHandle != IntPtr.Zero ? new Field(theHandle) : null;
        }
        public void Remove(int Index)
        {
            RawAPI.GRFieldsRemove(handle, Index);
        }
        public void RemoveAll()
        {
            RawAPI.GRFieldsRemoveAll(handle);
        }
    }

    public class ColumnContentCells : Base
    {
        public ColumnContentCells(IntPtr _handle)
            : base(_handle)
        {
        }
        public int Count
        {
            get
            {
                return RawAPI.GRColumnContentCellsGetCount(handle);
            }
        }

        public int IndexByName(string Name)
        {
            return RawAPI.GRColumnContentCellsIndexByName(handle, Name);
        }
        public ColumnContentCell ItemAt(int Index)
        {
            IntPtr theHandle = RawAPI.GRColumnContentCellsItemAt(handle, Index);
            return theHandle != IntPtr.Zero ? new ColumnContentCell(theHandle) : null;
        }
    }

    public class ColumnTitleCells : Base
    {
        public ColumnTitleCells(IntPtr _handle)
            : base(_handle)
        {
        }
        public int Count
        {
            get
            {
                return RawAPI.GRColumnTitleCellsGetCount(handle);
            }
        }

        public ColumnTitleCell AddGroup(string Name, string Title)
        {
            IntPtr theHandle = RawAPI.GRColumnTitleCellsAddGroup(handle, Name, Title);
            return theHandle != IntPtr.Zero ? new ColumnTitleCell(theHandle) : null;
        }
        public void ChangeItemOrder(int Index, int NewOrder)
        {
            RawAPI.GRColumnTitleCellsChangeItemOrder(handle, Index, NewOrder);
        }
        public int IndexByName(string Name)
        {
            return RawAPI.GRColumnTitleCellsIndexByName(handle, Name);
        }
        public ColumnTitleCell ItemAt(int Index)
        {
            IntPtr theHandle = RawAPI.GRColumnTitleCellsItemAt(handle, Index);
            return theHandle != IntPtr.Zero ? new ColumnTitleCell(theHandle) : null;
        }
        public void RemoveGroup(int Index)
        {
            RawAPI.GRColumnTitleCellsRemoveGroup(handle, Index);
        }
    }

    public class Columns : Base
    {
        public Columns(IntPtr _handle)
            : base(_handle)
        {
        }
        public int Count
        {
            get
            {
                return RawAPI.GRColumnsGetCount(handle);
            }
        }

        public Column Add()
        {
            IntPtr theHandle = RawAPI.GRColumnsAdd(handle);
            return theHandle != IntPtr.Zero ? new Column(theHandle) : null;
        }
        public void ChangeItemOrder(int Index, int NewOrder)
        {
            RawAPI.GRColumnsChangeItemOrder(handle, Index, NewOrder);
        }
        public int IndexByName(string Name)
        {
            return RawAPI.GRColumnsIndexByName(handle, Name);
        }
        public Column ItemAt(int Index)
        {
            IntPtr theHandle = RawAPI.GRColumnsItemAt(handle, Index);
            return theHandle != IntPtr.Zero ? new Column(theHandle) : null;
        }
        public void Remove(int Index)
        {
            RawAPI.GRColumnsRemove(handle, Index);
        }
        public void RemoveAll()
        {
            RawAPI.GRColumnsRemoveAll(handle);
        }
    }

    public class Groups : Base
    {
        public Groups(IntPtr _handle)
            : base(_handle)
        {
        }
        public int Count
        {
            get
            {
                return RawAPI.GRGroupsGetCount(handle);
            }
        }

        public Group Add()
        {
            IntPtr theHandle = RawAPI.GRGroupsAdd(handle);
            return theHandle != IntPtr.Zero ? new Group(theHandle) : null;
        }
        public void ChangeItemOrder(int Index, int NewOrder)
        {
            RawAPI.GRGroupsChangeItemOrder(handle, Index, NewOrder);
        }
        public int IndexByName(string Name)
        {
            return RawAPI.GRGroupsIndexByName(handle, Name);
        }
        public Group ItemAt(int Index)
        {
            IntPtr theHandle = RawAPI.GRGroupsItemAt(handle, Index);
            return theHandle != IntPtr.Zero ? new Group(theHandle) : null;
        }
        public void Remove(int Index)
        {
            RawAPI.GRGroupsRemove(handle, Index);
        }
        public void RemoveAll()
        {
            RawAPI.GRGroupsRemoveAll(handle);
        }
    }

    public class Parameters : Base
    {
        public Parameters(IntPtr _handle)
            : base(_handle)
        {
        }
        public int Count
        {
            get
            {
                return RawAPI.GRParametersGetCount(handle);
            }
        }

        public Parameter Add()
        {
            IntPtr theHandle = RawAPI.GRParametersAdd(handle);
            return theHandle != IntPtr.Zero ? new Parameter(theHandle) : null;
        }
        public void ChangeItemOrder(int Index, int NewOrder)
        {
            RawAPI.GRParametersChangeItemOrder(handle, Index, NewOrder);
        }
        public int IndexByName(string Name)
        {
            return RawAPI.GRParametersIndexByName(handle, Name);
        }
        public Parameter ItemAt(int Index)
        {
            IntPtr theHandle = RawAPI.GRParametersItemAt(handle, Index);
            return theHandle != IntPtr.Zero ? new Parameter(theHandle) : null;
        }
        public void Remove(int Index)
        {
            RawAPI.GRParametersRemove(handle, Index);
        }
        public void RemoveAll()
        {
            RawAPI.GRParametersRemoveAll(handle);
        }
    }

    public class ChartSeriess : Base
    {
        public ChartSeriess(IntPtr _handle)
            : base(_handle)
        {
        }
        public int Count
        {
            get
            {
                return RawAPI.GRChartSeriessGetCount(handle);
            }
        }

        public ChartSeries Add()
        {
            IntPtr theHandle = RawAPI.GRChartSeriessAdd(handle);
            return theHandle != IntPtr.Zero ? new ChartSeries(theHandle) : null;
        }
        public void ChangeItemOrder(int Index, int NewOrder)
        {
            RawAPI.GRChartSeriessChangeItemOrder(handle, Index, NewOrder);
        }
        public int IndexByName(string Name)
        {
            return RawAPI.GRChartSeriessIndexByName(handle, Name);
        }
        public ChartSeries ItemAt(int Index)
        {
            IntPtr theHandle = RawAPI.GRChartSeriessItemAt(handle, Index);
            return theHandle != IntPtr.Zero ? new ChartSeries(theHandle) : null;
        }
        public void Remove(int Index)
        {
            RawAPI.GRChartSeriessRemove(handle, Index);
        }
        public void RemoveAll()
        {
            RawAPI.GRChartSeriessRemoveAll(handle);
        }
    }

    public class ReportFooters : Base
    {
        public ReportFooters(IntPtr _handle)
            : base(_handle)
        {
        }
        public int Count
        {
            get
            {
                return RawAPI.GRReportFootersGetCount(handle);
            }
        }

        public ReportFooter Add()
        {
            IntPtr theHandle = RawAPI.GRReportFootersAdd(handle);
            return theHandle != IntPtr.Zero ? new ReportFooter(theHandle) : null;
        }
        public void ChangeItemOrder(int Index, int NewOrder)
        {
            RawAPI.GRReportFootersChangeItemOrder(handle, Index, NewOrder);
        }
        public int IndexByName(string Name)
        {
            return RawAPI.GRReportFootersIndexByName(handle, Name);
        }
        public ReportFooter ItemAt(int Index)
        {
            IntPtr theHandle = RawAPI.GRReportFootersItemAt(handle, Index);
            return theHandle != IntPtr.Zero ? new ReportFooter(theHandle) : null;
        }
        public void Remove(int Index)
        {
            RawAPI.GRReportFootersRemove(handle, Index);
        }
        public void RemoveAll()
        {
            RawAPI.GRReportFootersRemoveAll(handle);
        }
    }

    public class ReportHeaders : Base
    {
        public ReportHeaders(IntPtr _handle)
            : base(_handle)
        {
        }
        public int Count
        {
            get
            {
                return RawAPI.GRReportHeadersGetCount(handle);
            }
        }

        public ReportHeader Add()
        {
            IntPtr theHandle = RawAPI.GRReportHeadersAdd(handle);
            return theHandle != IntPtr.Zero ? new ReportHeader(theHandle) : null;
        }
        public void ChangeItemOrder(int Index, int NewOrder)
        {
            RawAPI.GRReportHeadersChangeItemOrder(handle, Index, NewOrder);
        }
        public int IndexByName(string Name)
        {
            return RawAPI.GRReportHeadersIndexByName(handle, Name);
        }
        public ReportHeader ItemAt(int Index)
        {
            IntPtr theHandle = RawAPI.GRReportHeadersItemAt(handle, Index);
            return theHandle != IntPtr.Zero ? new ReportHeader(theHandle) : null;
        }
        public void Remove(int Index)
        {
            RawAPI.GRReportHeadersRemove(handle, Index);
        }
        public void RemoveAll()
        {
            RawAPI.GRReportHeadersRemoveAll(handle);
        }
    }

    public class ExportOption : Base
    {
        public ExportOption(IntPtr _handle)
            : base(_handle)
        {
        }
        public E2CellOption AsE2CellOption
        {
            get
            {
                IntPtr theHandle = RawAPI.GRExportOptionGetAsE2CellOption(handle);
                return theHandle != IntPtr.Zero ? new E2CellOption(theHandle) : null;
            }
        }
        public E2CSVOption AsE2CSVOption
        {
            get
            {
                IntPtr theHandle = RawAPI.GRExportOptionGetAsE2CSVOption(handle);
                return theHandle != IntPtr.Zero ? new E2CSVOption(theHandle) : null;
            }
        }
        public E2HTMOption AsE2HTMOption
        {
            get
            {
                IntPtr theHandle = RawAPI.GRExportOptionGetAsE2HTMOption(handle);
                return theHandle != IntPtr.Zero ? new E2HTMOption(theHandle) : null;
            }
        }
        public E2IMGOption AsE2IMGOption
        {
            get
            {
                IntPtr theHandle = RawAPI.GRExportOptionGetAsE2IMGOption(handle);
                return theHandle != IntPtr.Zero ? new E2IMGOption(theHandle) : null;
            }
        }
        public E2PDFOption AsE2PDFOption
        {
            get
            {
                IntPtr theHandle = RawAPI.GRExportOptionGetAsE2PDFOption(handle);
                return theHandle != IntPtr.Zero ? new E2PDFOption(theHandle) : null;
            }
        }
        public E2RTFOption AsE2RTFOption
        {
            get
            {
                IntPtr theHandle = RawAPI.GRExportOptionGetAsE2RTFOption(handle);
                return theHandle != IntPtr.Zero ? new E2RTFOption(theHandle) : null;
            }
        }
        public E2TXTOption AsE2TXTOption
        {
            get
            {
                IntPtr theHandle = RawAPI.GRExportOptionGetAsE2TXTOption(handle);
                return theHandle != IntPtr.Zero ? new E2TXTOption(theHandle) : null;
            }
        }
        public E2XLSOption AsE2XLSOption
        {
            get
            {
                IntPtr theHandle = RawAPI.GRExportOptionGetAsE2XLSOption(handle);
                return theHandle != IntPtr.Zero ? new E2XLSOption(theHandle) : null;
            }
        }
        public ExportType ExportType
        {
            get
            {
                return RawAPI.GRExportOptionGetExportType(handle);
            }
        }
        public string FileName
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRExportOptionGetFileName(handle));
            }
            set
            {
                RawAPI.GRExportOptionSetFileName(handle, value);
            }
        }
        public string MailSubject
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRExportOptionGetMailSubject(handle));
            }
            set
            {
                RawAPI.GRExportOptionSetMailSubject(handle, value);
            }
        }
        public string MailText
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRExportOptionGetMailText(handle));
            }
            set
            {
                RawAPI.GRExportOptionSetMailText(handle, value);
            }
        }
        public string MailTo
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRExportOptionGetMailTo(handle));
            }
            set
            {
                RawAPI.GRExportOptionSetMailTo(handle, value);
            }
        }

    }

    public class E2CellOption : ExportOption
    {
        public E2CellOption(IntPtr _handle)
            : base(_handle)
        {
        }
        public bool ColumnAsDetailGrid
        {
            get
            {
                return RawAPI.GRE2CellOptionGetColumnAsDetailGrid(handle);
            }
            set
            {
                RawAPI.GRE2CellOptionSetColumnAsDetailGrid(handle, value);
            }
        }
        public bool ColumnTitleForbidRepeat
        {
            get
            {
                return RawAPI.GRE2CellOptionGetColumnTitleForbidRepeat(handle);
            }
            set
            {
                RawAPI.GRE2CellOptionSetColumnTitleForbidRepeat(handle, value);
            }
        }
        public bool ExportPageBreak
        {
            get
            {
                return RawAPI.GRE2CellOptionGetExportPageBreak(handle);
            }
            set
            {
                RawAPI.GRE2CellOptionSetExportPageBreak(handle, value);
            }
        }
        public bool ExportPageHeaderFooter
        {
            get
            {
                return RawAPI.GRE2CellOptionGetExportPageHeaderFooter(handle);
            }
            set
            {
                RawAPI.GRE2CellOptionSetExportPageHeaderFooter(handle, value);
            }
        }
        public bool OnlyExportDetailGrid
        {
            get
            {
                return RawAPI.GRE2CellOptionGetOnlyExportDetailGrid(handle);
            }
            set
            {
                RawAPI.GRE2CellOptionSetOnlyExportDetailGrid(handle, value);
            }
        }
        public bool OnlyExportPureText
        {
            get
            {
                return RawAPI.GRE2CellOptionGetOnlyExportPureText(handle);
            }
            set
            {
                RawAPI.GRE2CellOptionSetOnlyExportPureText(handle, value);
            }
        }
        public bool SameAsPrint
        {
            get
            {
                return RawAPI.GRE2CellOptionGetSameAsPrint(handle);
            }
            set
            {
                RawAPI.GRE2CellOptionSetSameAsPrint(handle, value);
            }
        }
        public bool SupressEmptyLines
        {
            get
            {
                return RawAPI.GRE2CellOptionGetSupressEmptyLines(handle);
            }
            set
            {
                RawAPI.GRE2CellOptionSetSupressEmptyLines(handle, value);
            }
        }

    }

    public class E2XLSOption : E2CellOption
    {
        public E2XLSOption(IntPtr _handle)
            : base(_handle)
        {
        }
        public bool ExpandRowHeight
        {
            get
            {
                return RawAPI.GRE2XLSOptionGetExpandRowHeight(handle);
            }
            set
            {
                RawAPI.GRE2XLSOptionSetExpandRowHeight(handle, value);
            }
        }
        public int MinColumnWidth
        {
            get
            {
                return RawAPI.GRE2XLSOptionGetMinColumnWidth(handle);
            }
            set
            {
                RawAPI.GRE2XLSOptionSetMinColumnWidth(handle, value);
            }
        }
        public int MinRowHeight
        {
            get
            {
                return RawAPI.GRE2XLSOptionGetMinRowHeight(handle);
            }
            set
            {
                RawAPI.GRE2XLSOptionSetMinRowHeight(handle, value);
            }
        }
        public int NewSheetRows
        {
            get
            {
                return RawAPI.GRE2XLSOptionGetNewSheetRows(handle);
            }
            set
            {
                RawAPI.GRE2XLSOptionSetNewSheetRows(handle, value);
            }
        }
        public double PageBottomMargin
        {
            get
            {
                return RawAPI.GRE2XLSOptionGetPageBottomMargin(handle);
            }
            set
            {
                RawAPI.GRE2XLSOptionSetPageBottomMargin(handle, value);
            }
        }
        public double PageLeftMargin
        {
            get
            {
                return RawAPI.GRE2XLSOptionGetPageLeftMargin(handle);
            }
            set
            {
                RawAPI.GRE2XLSOptionSetPageLeftMargin(handle, value);
            }
        }
        public double PageRightMargin
        {
            get
            {
                return RawAPI.GRE2XLSOptionGetPageRightMargin(handle);
            }
            set
            {
                RawAPI.GRE2XLSOptionSetPageRightMargin(handle, value);
            }
        }
        public double PageTopMargin
        {
            get
            {
                return RawAPI.GRE2XLSOptionGetPageTopMargin(handle);
            }
            set
            {
                RawAPI.GRE2XLSOptionSetPageTopMargin(handle, value);
            }
        }
        public bool ToXlsxFormat
        {
            get
            {
                return RawAPI.GRE2XLSOptionGetToXlsxFormat(handle);
            }
            set
            {
                RawAPI.GRE2XLSOptionSetToXlsxFormat(handle, value);
            }
        }

    }

    public class E2HTMOption : E2CellOption
    {
        public E2HTMOption(IntPtr _handle)
            : base(_handle)
        {
        }
        public string PicturePath
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRE2HTMOptionGetPicturePath(handle));
            }
            set
            {
                RawAPI.GRE2HTMOptionSetPicturePath(handle, value);
            }
        }

    }

    public class E2TXTOption : E2CellOption
    {
        public E2TXTOption(IntPtr _handle)
            : base(_handle)
        {
        }
        public TextEncodeMode TextEncode
        {
            get
            {
                return RawAPI.GRE2TXTOptionGetTextEncode(handle);
            }
            set
            {
                RawAPI.GRE2TXTOptionSetTextEncode(handle, value);
            }
        }
        public bool UsingTabChar
        {
            get
            {
                return RawAPI.GRE2TXTOptionGetUsingTabChar(handle);
            }
            set
            {
                RawAPI.GRE2TXTOptionSetUsingTabChar(handle, value);
            }
        }

    }

    public class E2CSVOption : E2CellOption
    {
        public E2CSVOption(IntPtr _handle)
            : base(_handle)
        {
        }
        public string DelimiterChar
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRE2CSVOptionGetDelimiterChar(handle));
            }
            set
            {
                RawAPI.GRE2CSVOptionSetDelimiterChar(handle, value);
            }
        }
        public string QuoteText
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRE2CSVOptionGetQuoteText(handle));
            }
            set
            {
                RawAPI.GRE2CSVOptionSetQuoteText(handle, value);
            }
        }
        public TextEncodeMode TextEncode
        {
            get
            {
                return RawAPI.GRE2CSVOptionGetTextEncode(handle);
            }
            set
            {
                RawAPI.GRE2CSVOptionSetTextEncode(handle, value);
            }
        }

    }

    public class E2RTFOption : E2CellOption
    {
        public E2RTFOption(IntPtr _handle)
            : base(_handle)
        {
        }
        public string Author
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRE2RTFOptionGetAuthor(handle));
            }
            set
            {
                RawAPI.GRE2RTFOptionSetAuthor(handle, value);
            }
        }
        public string Creator
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRE2RTFOptionGetCreator(handle));
            }
            set
            {
                RawAPI.GRE2RTFOptionSetCreator(handle, value);
            }
        }

    }

    public class E2PDFOption : ExportOption
    {
        public E2PDFOption(IntPtr _handle)
            : base(_handle)
        {
        }
        public string Author
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRE2PDFOptionGetAuthor(handle));
            }
            set
            {
                RawAPI.GRE2PDFOptionSetAuthor(handle, value);
            }
        }
        public bool Compressed
        {
            get
            {
                return RawAPI.GRE2PDFOptionGetCompressed(handle);
            }
            set
            {
                RawAPI.GRE2PDFOptionSetCompressed(handle, value);
            }
        }
        public string Creator
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRE2PDFOptionGetCreator(handle));
            }
            set
            {
                RawAPI.GRE2PDFOptionSetCreator(handle, value);
            }
        }
        public bool FontEmbedding
        {
            get
            {
                return RawAPI.GRE2PDFOptionGetFontEmbedding(handle);
            }
            set
            {
                RawAPI.GRE2PDFOptionSetFontEmbedding(handle, value);
            }
        }
        public string Keywords
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRE2PDFOptionGetKeywords(handle));
            }
            set
            {
                RawAPI.GRE2PDFOptionSetKeywords(handle, value);
            }
        }
        public string OwnerPassword
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRE2PDFOptionGetOwnerPassword(handle));
            }
            set
            {
                RawAPI.GRE2PDFOptionSetOwnerPassword(handle, value);
            }
        }
        public string Producer
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRE2PDFOptionGetProducer(handle));
            }
            set
            {
                RawAPI.GRE2PDFOptionSetProducer(handle, value);
            }
        }
        public string Subject
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRE2PDFOptionGetSubject(handle));
            }
            set
            {
                RawAPI.GRE2PDFOptionSetSubject(handle, value);
            }
        }
        public string UserPassword
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRE2PDFOptionGetUserPassword(handle));
            }
            set
            {
                RawAPI.GRE2PDFOptionSetUserPassword(handle, value);
            }
        }

        public bool GetDocPermission(PDFDocPermission Permission)
        {
            return RawAPI.GRE2PDFOptionGetDocPermission(handle, Permission);
        }
        public void SetDocPermission(PDFDocPermission Permission, bool Enabled)
        {
            RawAPI.GRE2PDFOptionSetDocPermission(handle, Permission, Enabled);
        }
    }

    public class E2IMGOption : ExportOption
    {
        public E2IMGOption(IntPtr _handle)
            : base(_handle)
        {
        }
        public bool AllInOne
        {
            get
            {
                return RawAPI.GRE2IMGOptionGetAllInOne(handle);
            }
            set
            {
                RawAPI.GRE2IMGOptionSetAllInOne(handle, value);
            }
        }
        public int DPI
        {
            get
            {
                return RawAPI.GRE2IMGOptionGetDPI(handle);
            }
            set
            {
                RawAPI.GRE2IMGOptionSetDPI(handle, value);
            }
        }
        public bool DrawPageBox
        {
            get
            {
                return RawAPI.GRE2IMGOptionGetDrawPageBox(handle);
            }
            set
            {
                RawAPI.GRE2IMGOptionSetDrawPageBox(handle, value);
            }
        }
        public ExportImageType ImageType
        {
            get
            {
                return RawAPI.GRE2IMGOptionGetImageType(handle);
            }
            set
            {
                RawAPI.GRE2IMGOptionSetImageType(handle, value);
            }
        }
        public int VertGap
        {
            get
            {
                return RawAPI.GRE2IMGOptionGetVertGap(handle);
            }
            set
            {
                RawAPI.GRE2IMGOptionSetVertGap(handle, value);
            }
        }

    }

    public class Printer : Base
    {
        public Printer(IntPtr _handle)
            : base(_handle)
        {
        }
        public double BottomMargin
        {
            get
            {
                return RawAPI.GRPrinterGetBottomMargin(handle);
            }
            set
            {
                RawAPI.GRPrinterSetBottomMargin(handle, value);
            }
        }
        public bool CanCollate
        {
            get
            {
                return RawAPI.GRPrinterGetCanCollate(handle);
            }
        }
        public bool CanDuplex
        {
            get
            {
                return RawAPI.GRPrinterGetCanDuplex(handle);
            }
        }
        public bool Collate
        {
            get
            {
                return RawAPI.GRPrinterGetCollate(handle);
            }
            set
            {
                RawAPI.GRPrinterSetCollate(handle, value);
            }
        }
        public int Copies
        {
            get
            {
                return RawAPI.GRPrinterGetCopies(handle);
            }
            set
            {
                RawAPI.GRPrinterSetCopies(handle, value);
            }
        }
        public int CurPageNo
        {
            get
            {
                return RawAPI.GRPrinterGetCurPageNo(handle);
            }
            set
            {
                RawAPI.GRPrinterSetCurPageNo(handle, value);
            }
        }
        public double DesignBottomMargin
        {
            get
            {
                return RawAPI.GRPrinterGetDesignBottomMargin(handle);
            }
            set
            {
                RawAPI.GRPrinterSetDesignBottomMargin(handle, value);
            }
        }
        public double DesignLeftMargin
        {
            get
            {
                return RawAPI.GRPrinterGetDesignLeftMargin(handle);
            }
            set
            {
                RawAPI.GRPrinterSetDesignLeftMargin(handle, value);
            }
        }
        public double DesignPaperLength
        {
            get
            {
                return RawAPI.GRPrinterGetDesignPaperLength(handle);
            }
            set
            {
                RawAPI.GRPrinterSetDesignPaperLength(handle, value);
            }
        }
        public string DesignPaperName
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRPrinterGetDesignPaperName(handle));
            }
            set
            {
                RawAPI.GRPrinterSetDesignPaperName(handle, value);
            }
        }
        public PaperOrientation DesignPaperOrientation
        {
            get
            {
                return RawAPI.GRPrinterGetDesignPaperOrientation(handle);
            }
            set
            {
                RawAPI.GRPrinterSetDesignPaperOrientation(handle, value);
            }
        }
        public int DesignPaperSize
        {
            get
            {
                return RawAPI.GRPrinterGetDesignPaperSize(handle);
            }
            set
            {
                RawAPI.GRPrinterSetDesignPaperSize(handle, value);
            }
        }
        public double DesignPaperWidth
        {
            get
            {
                return RawAPI.GRPrinterGetDesignPaperWidth(handle);
            }
            set
            {
                RawAPI.GRPrinterSetDesignPaperWidth(handle, value);
            }
        }
        public string DesignPrinterName
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRPrinterGetDesignPrinterName(handle));
            }
            set
            {
                RawAPI.GRPrinterSetDesignPrinterName(handle, value);
            }
        }
        public double DesignRightMargin
        {
            get
            {
                return RawAPI.GRPrinterGetDesignRightMargin(handle);
            }
            set
            {
                RawAPI.GRPrinterSetDesignRightMargin(handle, value);
            }
        }
        public double DesignTopMargin
        {
            get
            {
                return RawAPI.GRPrinterGetDesignTopMargin(handle);
            }
            set
            {
                RawAPI.GRPrinterSetDesignTopMargin(handle, value);
            }
        }
        public DuplexKind Duplex
        {
            get
            {
                return RawAPI.GRPrinterGetDuplex(handle);
            }
            set
            {
                RawAPI.GRPrinterSetDuplex(handle, value);
            }
        }
        public double LeftMargin
        {
            get
            {
                return RawAPI.GRPrinterGetLeftMargin(handle);
            }
            set
            {
                RawAPI.GRPrinterSetLeftMargin(handle, value);
            }
        }
        public bool Online
        {
            get
            {
                return RawAPI.GRPrinterGetOnline(handle);
            }
        }
        public int PageCount
        {
            get
            {
                return RawAPI.GRPrinterGetPageCount(handle);
            }
            set
            {
                RawAPI.GRPrinterSetPageCount(handle, value);
            }
        }
        public double PaperLength
        {
            get
            {
                return RawAPI.GRPrinterGetPaperLength(handle);
            }
            set
            {
                RawAPI.GRPrinterSetPaperLength(handle, value);
            }
        }
        public string PaperName
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRPrinterGetPaperName(handle));
            }
            set
            {
                RawAPI.GRPrinterSetPaperName(handle, value);
            }
        }
        public PaperOrientation PaperOrientation
        {
            get
            {
                return RawAPI.GRPrinterGetPaperOrientation(handle);
            }
            set
            {
                RawAPI.GRPrinterSetPaperOrientation(handle, value);
            }
        }
        public int PaperSize
        {
            get
            {
                return RawAPI.GRPrinterGetPaperSize(handle);
            }
            set
            {
                RawAPI.GRPrinterSetPaperSize(handle, value);
            }
        }
        public PaperSourceKind PaperSource
        {
            get
            {
                return RawAPI.GRPrinterGetPaperSource(handle);
            }
            set
            {
                RawAPI.GRPrinterSetPaperSource(handle, value);
            }
        }
        public double PaperWidth
        {
            get
            {
                return RawAPI.GRPrinterGetPaperWidth(handle);
            }
            set
            {
                RawAPI.GRPrinterSetPaperWidth(handle, value);
            }
        }
        public string PrinterName
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRPrinterGetPrinterName(handle));
            }
            set
            {
                RawAPI.GRPrinterSetPrinterName(handle, value);
            }
        }
        public bool PrintOffsetSaveToLocal
        {
            get
            {
                return RawAPI.GRPrinterGetPrintOffsetSaveToLocal(handle);
            }
            set
            {
                RawAPI.GRPrinterSetPrintOffsetSaveToLocal(handle, value);
            }
        }
        public double PrintOffsetX
        {
            get
            {
                return RawAPI.GRPrinterGetPrintOffsetX(handle);
            }
            set
            {
                RawAPI.GRPrinterSetPrintOffsetX(handle, value);
            }
        }
        public double PrintOffsetY
        {
            get
            {
                return RawAPI.GRPrinterGetPrintOffsetY(handle);
            }
            set
            {
                RawAPI.GRPrinterSetPrintOffsetY(handle, value);
            }
        }
        public PrintPageType PrintPageType
        {
            get
            {
                return RawAPI.GRPrinterGetPrintPageType(handle);
            }
            set
            {
                RawAPI.GRPrinterSetPrintPageType(handle, value);
            }
        }
        public PrintRangeType PrintRangeType
        {
            get
            {
                return RawAPI.GRPrinterGetPrintRangeType(handle);
            }
            set
            {
                RawAPI.GRPrinterSetPrintRangeType(handle, value);
            }
        }
        public DrawRotation PrintRotation
        {
            get
            {
                return RawAPI.GRPrinterGetPrintRotation(handle);
            }
            set
            {
                RawAPI.GRPrinterSetPrintRotation(handle, value);
            }
        }
        public double RightMargin
        {
            get
            {
                return RawAPI.GRPrinterGetRightMargin(handle);
            }
            set
            {
                RawAPI.GRPrinterSetRightMargin(handle, value);
            }
        }
        public string SelectionPrintPages
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRPrinterGetSelectionPrintPages(handle));
            }
            set
            {
                RawAPI.GRPrinterSetSelectionPrintPages(handle, value);
            }
        }
        public SheetPages SheetPages
        {
            get
            {
                return RawAPI.GRPrinterGetSheetPages(handle);
            }
            set
            {
                RawAPI.GRPrinterSetSheetPages(handle, value);
            }
        }
        public int SheetPaperSize
        {
            get
            {
                return RawAPI.GRPrinterGetSheetPaperSize(handle);
            }
            set
            {
                RawAPI.GRPrinterSetSheetPaperSize(handle, value);
            }
        }
        public bool SupportCustomPaper
        {
            get
            {
                return RawAPI.GRPrinterGetSupportCustomPaper(handle);
            }
        }
        public double TopMargin
        {
            get
            {
                return RawAPI.GRPrinterGetTopMargin(handle);
            }
            set
            {
                RawAPI.GRPrinterSetTopMargin(handle, value);
            }
        }

        public void SetCustomPaperSize(double Width, double Length)
        {
            RawAPI.GRPrinterSetCustomPaperSize(handle, Width, Length);
        }
    }

    public class ImageList : Base
    {
        public ImageList(IntPtr _handle)
            : base(_handle)
        {
        }
        public int Count
        {
            get
            {
                return RawAPI.GRImageListGetCount(handle);
            }
        }

        public int AddFromBinary(BinaryObject BinaryObject)
        {
            return RawAPI.GRImageListAddFromBinary(handle, BinaryObject.NativeHandle);
        }
        public int AddFromFile(string PathFile)
        {
            return RawAPI.GRImageListAddFromFile(handle, PathFile);
        }
        public Picture Item(int Index)
        {
            IntPtr theHandle = RawAPI.GRImageListItem(handle, Index);
            return theHandle != IntPtr.Zero ? new Picture(theHandle) : null;
        }
        public void Remove(int Index)
        {
            RawAPI.GRImageListRemove(handle, Index);
        }
        public void RemoveAll()
        {
            RawAPI.GRImageListRemoveAll(handle);
        }
    }

    public class ReportLinux : Object
    {
        private bool selfCreated = true;

        public ReportLinux()
            : base(IntPtr.Zero)
        {
            InitConfirm();
            handle = RawAPI.GRReportCreate();
        }
        public ReportLinux(IntPtr _handle)
            : base(_handle)
        {
            selfCreated = false;
            InitConfirm();
        }
        ~ReportLinux()
        {
            Release();
        }

        public bool AbortOnError
        {
            get
            {
                return RawAPI.GRReportGetAbortOnError(handle);
            }
            set
            {
                RawAPI.GRReportSetAbortOnError(handle, value);
            }
        }
        public bool AlignToGrid
        {
            get
            {
                return RawAPI.GRReportGetAlignToGrid(handle);
            }
            set
            {
                RawAPI.GRReportSetAlignToGrid(handle, value);
            }
        }
        public string Author
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetAuthor(handle));
            }
            set
            {
                RawAPI.GRReportSetAuthor(handle, value);
            }
        }
        public uint BackColor
        {
            get
            {
                return RawAPI.GRReportGetBackColor(handle);
            }
            set
            {
                RawAPI.GRReportSetBackColor(handle, value);
            }
        }
        public Picture BackImage
        {
            get
            {
                IntPtr theHandle = RawAPI.GRReportGetBackImage(handle);
                return theHandle != IntPtr.Zero ? new Picture(theHandle) : null;
            }
            set
            {
                RawAPI.GRReportSetBackImage(handle, value != null ? value.NativeHandle : IntPtr.Zero);
            }
        }
        public string BackImageFile
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetBackImageFile(handle));
            }
            set
            {
                RawAPI.GRReportSetBackImageFile(handle, value);
            }
        }
        public bool BackImagePreview
        {
            get
            {
                return RawAPI.GRReportGetBackImagePreview(handle);
            }
            set
            {
                RawAPI.GRReportSetBackImagePreview(handle, value);
            }
        }
        public bool BackImagePrint
        {
            get
            {
                return RawAPI.GRReportGetBackImagePrint(handle);
            }
            set
            {
                RawAPI.GRReportSetBackImagePrint(handle, value);
            }
        }
        public string BeforeSortScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetBeforeSortScript(handle));
            }
            set
            {
                RawAPI.GRReportSetBeforeSortScript(handle, value);
            }
        }
        public int CodePage
        {
            get
            {
                return RawAPI.GRReportGetCodePage(handle);
            }
            set
            {
                RawAPI.GRReportSetCodePage(handle, value);
            }
        }
        public string ConnectionString
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetConnectionString(handle));
            }
            set
            {
                RawAPI.GRReportSetConnectionString(handle, value);
            }
        }
        public bool ContinuePrint
        {
            get
            {
                return RawAPI.GRReportGetContinuePrint(handle);
            }
            set
            {
                RawAPI.GRReportSetContinuePrint(handle, value);
            }
        }
        public bool DataLoaded
        {
            get
            {
                return RawAPI.GRReportGetDataLoaded(handle);
            }
        }
        public string Description
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetDescription(handle));
            }
            set
            {
                RawAPI.GRReportSetDescription(handle, value);
            }
        }
        public DetailGrid DetailGrid
        {
            get
            {
                IntPtr theHandle = RawAPI.GRReportGetDetailGrid(handle);
                return theHandle != IntPtr.Zero ? new DetailGrid(theHandle) : null;
            }
        }
        public ReportDisplayMode DisplayMode
        {
            get
            {
                return RawAPI.GRReportGetDisplayMode(handle);
            }
        }
        public DocType DocType
        {
            get
            {
                return RawAPI.GRReportGetDocType(handle);
            }
            set
            {
                RawAPI.GRReportSetDocType(handle, value);
            }
        }
        public string ExportBeginScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetExportBeginScript(handle));
            }
            set
            {
                RawAPI.GRReportSetExportBeginScript(handle, value);
            }
        }
        //public Report FiringReport
        //{
        //    get
        //    {
        //        IntPtr theHandle = RawAPI.GRReportGetFiringReport(handle);
        //        return theHandle != IntPtr.Zero ? new Report(theHandle) : null;
        //    }
        //}
        public bool FirstPass
        {
            get
            {
                return RawAPI.GRReportGetFirstPass(handle);
            }
        }
        public Controls FloatControls
        {
            get
            {
                IntPtr theHandle = RawAPI.GRReportGetFloatControls(handle);
                return theHandle != IntPtr.Zero ? new Controls(theHandle) : null;
            }
        }
        public Font Font
        {
            get
            {
                IntPtr theHandle = RawAPI.GRReportGetFont(handle);
                return theHandle != IntPtr.Zero ? new Font(theHandle) : null;
            }
        }
        public string GeneratePagesBeginScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetGeneratePagesBeginScript(handle));
            }
            set
            {
                RawAPI.GRReportSetGeneratePagesBeginScript(handle, value);
            }
        }
        public string GeneratePagesEndScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetGeneratePagesEndScript(handle));
            }
            set
            {
                RawAPI.GRReportSetGeneratePagesEndScript(handle, value);
            }
        }
        public string GlobalScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetGlobalScript(handle));
            }
            set
            {
                RawAPI.GRReportSetGlobalScript(handle, value);
            }
        }
        public Graphics Graphics
        {
            get
            {
                IntPtr theHandle = RawAPI.GRReportGetGraphics(handle);
                return theHandle != IntPtr.Zero ? new Graphics(theHandle) : null;
            }
        }
        public int GridColsPerUnit
        {
            get
            {
                return RawAPI.GRReportGetGridColsPerUnit(handle);
            }
            set
            {
                RawAPI.GRReportSetGridColsPerUnit(handle, value);
            }
        }
        public int GridRowsPerUnit
        {
            get
            {
                return RawAPI.GRReportGetGridRowsPerUnit(handle);
            }
            set
            {
                RawAPI.GRReportSetGridRowsPerUnit(handle, value);
            }
        }
        public bool HasFloatControl
        {
            get
            {
                return RawAPI.GRReportGetHasFloatControl(handle);
            }
        }
        public ImageList ImageList
        {
            get
            {
                IntPtr theHandle = RawAPI.GRReportGetImageList(handle);
                return theHandle != IntPtr.Zero ? new ImageList(theHandle) : null;
            }
        }
        public string InitializeScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetInitializeScript(handle));
            }
            set
            {
                RawAPI.GRReportSetInitializeScript(handle, value);
            }
        }
        public bool IsBlank
        {
            get
            {
                return RawAPI.GRReportGetIsBlank(handle);
            }
        }
        public int Language
        {
            get
            {
                return RawAPI.GRReportGetLanguage(handle);
            }
            set
            {
                RawAPI.GRReportSetLanguage(handle, value);
            }
        }
        public string LatestLoadedFile
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetLatestLoadedFile(handle));
            }
            set
            {
                RawAPI.GRReportSetLatestLoadedFile(handle, value);
            }
        }
        public bool MirrorMargins
        {
            get
            {
                return RawAPI.GRReportGetMirrorMargins(handle);
            }
            set
            {
                RawAPI.GRReportSetMirrorMargins(handle, value);
            }
        }
        public bool MonoPrint
        {
            get
            {
                return RawAPI.GRReportGetMonoPrint(handle);
            }
            set
            {
                RawAPI.GRReportSetMonoPrint(handle, value);
            }
        }
        public double PageBlankHeight
        {
            get
            {
                return RawAPI.GRReportGetPageBlankHeight(handle);
            }
        }
        public int PageDivideCount
        {
            get
            {
                return RawAPI.GRReportGetPageDivideCount(handle);
            }
            set
            {
                RawAPI.GRReportSetPageDivideCount(handle, value);
            }
        }
        public bool PageDivideLine
        {
            get
            {
                return RawAPI.GRReportGetPageDivideLine(handle);
            }
            set
            {
                RawAPI.GRReportSetPageDivideLine(handle, value);
            }
        }
        public double PageDivideSpacing
        {
            get
            {
                return RawAPI.GRReportGetPageDivideSpacing(handle);
            }
            set
            {
                RawAPI.GRReportSetPageDivideSpacing(handle, value);
            }
        }
        public string PageEndScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetPageEndScript(handle));
            }
            set
            {
                RawAPI.GRReportSetPageEndScript(handle, value);
            }
        }
        public PageFooter PageFooter
        {
            get
            {
                IntPtr theHandle = RawAPI.GRReportGetPageFooter(handle);
                return theHandle != IntPtr.Zero ? new PageFooter(theHandle) : null;
            }
        }
        public PageHeader PageHeader
        {
            get
            {
                IntPtr theHandle = RawAPI.GRReportGetPageHeader(handle);
                return theHandle != IntPtr.Zero ? new PageHeader(theHandle) : null;
            }
        }
        public string PageStartScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetPageStartScript(handle));
            }
            set
            {
                RawAPI.GRReportSetPageStartScript(handle, value);
            }
        }
        public Parameters Parameters
        {
            get
            {
                IntPtr theHandle = RawAPI.GRReportGetParameters(handle);
                return theHandle != IntPtr.Zero ? new Parameters(theHandle) : null;
            }
        }
        public ReportLinux ParentReport
        {
            get
            {
                IntPtr theHandle = RawAPI.GRReportGetParentReport(handle);
                return theHandle != IntPtr.Zero ? new ReportLinux(theHandle) : null;
            }
        }
        public bool PrintAsDesignPaper
        {
            get
            {
                return RawAPI.GRReportGetPrintAsDesignPaper(handle);
            }
            set
            {
                RawAPI.GRReportSetPrintAsDesignPaper(handle, value);
            }
        }
        public Printer Printer
        {
            get
            {
                IntPtr theHandle = RawAPI.GRReportGetPrinter(handle);
                return theHandle != IntPtr.Zero ? new Printer(theHandle) : null;
            }
        }
        public string ProcessBeginScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetProcessBeginScript(handle));
            }
            set
            {
                RawAPI.GRReportSetProcessBeginScript(handle, value);
            }
        }
        public string ProcessEndScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetProcessEndScript(handle));
            }
            set
            {
                RawAPI.GRReportSetProcessEndScript(handle, value);
            }
        }
        public string QuerySQL
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetQuerySQL(handle));
            }
            set
            {
                RawAPI.GRReportSetQuerySQL(handle, value);
            }
        }
        public ReportFooters ReportFooters
        {
            get
            {
                IntPtr theHandle = RawAPI.GRReportGetReportFooters(handle);
                return theHandle != IntPtr.Zero ? new ReportFooters(theHandle) : null;
            }
        }
        public ReportHeaders ReportHeaders
        {
            get
            {
                IntPtr theHandle = RawAPI.GRReportGetReportHeaders(handle);
                return theHandle != IntPtr.Zero ? new ReportHeaders(theHandle) : null;
            }
        }
        public bool Running
        {
            get
            {
                return RawAPI.GRReportGetRunning(handle);
            }
        }
        public DetailGrid RunningDetailGrid
        {
            get
            {
                IntPtr theHandle = RawAPI.GRReportGetRunningDetailGrid(handle);
                return theHandle != IntPtr.Zero ? new DetailGrid(theHandle) : null;
            }
        }
        public ScriptType ScriptType
        {
            get
            {
                return RawAPI.GRReportGetScriptType(handle);
            }
            set
            {
                RawAPI.GRReportSetScriptType(handle, value);
            }
        }
        public bool ShowGrid
        {
            get
            {
                return RawAPI.GRReportGetShowGrid(handle);
            }
            set
            {
                RawAPI.GRReportSetShowGrid(handle, value);
            }
        }
        public int ShowMoneyDigit
        {
            get
            {
                return RawAPI.GRReportGetShowMoneyDigit(handle);
            }
            set
            {
                RawAPI.GRReportSetShowMoneyDigit(handle, value);
            }
        }
        public uint ShowMoneyLineColor
        {
            get
            {
                return RawAPI.GRReportGetShowMoneyLineColor(handle);
            }
            set
            {
                RawAPI.GRReportSetShowMoneyLineColor(handle, value);
            }
        }
        public bool ShowMoneySepLineColor
        {
            get
            {
                return RawAPI.GRReportGetShowMoneySepLineColor(handle);
            }
            set
            {
                RawAPI.GRReportSetShowMoneySepLineColor(handle, value);
            }
        }
        public double ShowMoneyWidth
        {
            get
            {
                return RawAPI.GRReportGetShowMoneyWidth(handle);
            }
            set
            {
                RawAPI.GRReportSetShowMoneyWidth(handle, value);
            }
        }
        public string ShowPreviewWndScript
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetShowPreviewWndScript(handle));
            }
            set
            {
                RawAPI.GRReportSetShowPreviewWndScript(handle, value);
            }
        }
        public bool SkipQuery
        {
            get
            {
                return RawAPI.GRReportGetSkipQuery(handle);
            }
            set
            {
                RawAPI.GRReportSetSkipQuery(handle, value);
            }
        }
        public StorageFormat StorageFormat
        {
            get
            {
                return RawAPI.GRReportGetStorageFormat(handle);
            }
            set
            {
                RawAPI.GRReportSetStorageFormat(handle, value);
            }
        }
        public TextEncodeMode TextEncode
        {
            get
            {
                return RawAPI.GRReportGetTextEncode(handle);
            }
            set
            {
                RawAPI.GRReportSetTextEncode(handle, value);
            }
        }
        public string Title
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetTitle(handle));
            }
            set
            {
                RawAPI.GRReportSetTitle(handle, value);
            }
        }
        public Unit Unit
        {
            get
            {
                return RawAPI.GRReportGetUnit(handle);
            }
            set
            {
                RawAPI.GRReportSetUnit(handle, value);
            }
        }
        public Utility Utility
        {
            get
            {
                IntPtr theHandle = RawAPI.GRReportGetUtility(handle);
                return theHandle != IntPtr.Zero ? new Utility(theHandle) : null;
            }
        }
        public string Version
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetVersion(handle));
            }
        }
        public Picture Watermark
        {
            get
            {
                IntPtr theHandle = RawAPI.GRReportGetWatermark(handle);
                return theHandle != IntPtr.Zero ? new Picture(theHandle) : null;
            }
            set
            {
                RawAPI.GRReportSetWatermark(handle, value != null ? value.NativeHandle : IntPtr.Zero);
            }
        }
        public PictureAlignment WatermarkAlignment
        {
            get
            {
                return RawAPI.GRReportGetWatermarkAlignment(handle);
            }
            set
            {
                RawAPI.GRReportSetWatermarkAlignment(handle, value);
            }
        }
        public PictureSizeMode WatermarkSizeMode
        {
            get
            {
                return RawAPI.GRReportGetWatermarkSizeMode(handle);
            }
            set
            {
                RawAPI.GRReportSetWatermarkSizeMode(handle, value);
            }
        }
        public string XmlTableName
        {
            get
            {
                return Marshal.PtrToStringUni(RawAPI.GRReportGetXmlTableName(handle));
            }
            set
            {
                RawAPI.GRReportSetXmlTableName(handle, value);
            }
        }

        public void Abort()
        {
            RawAPI.GRReportAbort(handle);
        }
        public void AbortExport()
        {
            RawAPI.GRReportAbortExport(handle);
        }
        public void AbortPrint()
        {
            RawAPI.GRReportAbortPrint(handle);
        }
        public Parameter AddParameter(string Name, ParameterDataType Type)
        {
            IntPtr theHandle = RawAPI.GRReportAddParameter(handle, Name, Type);
            return theHandle != IntPtr.Zero ? new Parameter(theHandle) : null;
        }
        public bool BeginLoopPrint(PrintGenerateStyle GenerateStyle, bool ShowPrintDlg)
        {
            return RawAPI.GRReportBeginLoopPrint(handle, GenerateStyle, ShowPrintDlg);
        }
        public void CancelLoadData()
        {
            RawAPI.GRReportCancelLoadData(handle);
        }
        public void Clear()
        {
            RawAPI.GRReportClear(handle);
        }
        public Column ColumnByName(string Name)
        {
            IntPtr theHandle = RawAPI.GRReportColumnByName(handle, Name);
            return theHandle != IntPtr.Zero ? new Column(theHandle) : null;
        }
        public Control ControlByName(string Name)
        {
            IntPtr theHandle = RawAPI.GRReportControlByName(handle, Name);
            return theHandle != IntPtr.Zero ? new Control(theHandle) : null;
        }
        public void DeleteDetailGrid()
        {
            RawAPI.GRReportDeleteDetailGrid(handle);
        }
        public void DeletePageFooter()
        {
            RawAPI.GRReportDeletePageFooter(handle);
        }
        public void DeletePageHeader()
        {
            RawAPI.GRReportDeletePageHeader(handle);
        }
        public void EndLoopPrint()
        {
            RawAPI.GRReportEndLoopPrint(handle);
        }
        public bool Export(bool DoneOpen)
        {
            return RawAPI.GRReportExport(handle, DoneOpen);
        }
        public bool ExportDirect(ExportType ExportType, string FileName, bool ShowOptionDlg, bool DoneOpen)
        {
            return RawAPI.GRReportExportDirect(handle, ExportType, FileName, ShowOptionDlg, DoneOpen);
        }
        public BinaryObject ExportDirectToBinaryObject(ExportType ExportType)
        {
            IntPtr theHandle = RawAPI.GRReportExportDirectToBinaryObject(handle, ExportType);
            return theHandle != IntPtr.Zero ? new BinaryObject(theHandle) : null;
        }
        public BinaryObject ExportToBinaryObject()
        {
            IntPtr theHandle = RawAPI.GRReportExportToBinaryObject(handle);
            return theHandle != IntPtr.Zero ? new BinaryObject(theHandle) : null;
        }
        public string ExtractXMLFromURL(string URL)
        {
            return Marshal.PtrToStringUni(RawAPI.GRReportExtractXMLFromURL(handle, URL));
        }
        public Field FieldByDBName(string DBFieldName)
        {
            IntPtr theHandle = RawAPI.GRReportFieldByDBName(handle, DBFieldName);
            return theHandle != IntPtr.Zero ? new Field(theHandle) : null;
        }
        public Field FieldByName(string Name)
        {
            IntPtr theHandle = RawAPI.GRReportFieldByName(handle, Name);
            return theHandle != IntPtr.Zero ? new Field(theHandle) : null;
        }
        public Control FindFirstControl()
        {
            IntPtr theHandle = RawAPI.GRReportFindFirstControl(handle);
            return theHandle != IntPtr.Zero ? new Control(theHandle) : null;
        }
        public Control FindNextControl()
        {
            IntPtr theHandle = RawAPI.GRReportFindNextControl(handle);
            return theHandle != IntPtr.Zero ? new Control(theHandle) : null;
        }
        public void ForceNewPage()
        {
            RawAPI.GRReportForceNewPage(handle);
        }
        public BinaryObject GenerateDocumentData()
        {
            IntPtr theHandle = RawAPI.GRReportGenerateDocumentData(handle);
            return theHandle != IntPtr.Zero ? new BinaryObject(theHandle) : null;
        }
        public void GenerateDocumentFile(string PathFile)
        {
            RawAPI.GRReportGenerateDocumentFile(handle, PathFile);
        }
        public DetailGrid InsertDetailGrid()
        {
            IntPtr theHandle = RawAPI.GRReportInsertDetailGrid(handle);
            return theHandle != IntPtr.Zero ? new DetailGrid(theHandle) : null;
        }
        public PageFooter InsertPageFooter()
        {
            IntPtr theHandle = RawAPI.GRReportInsertPageFooter(handle);
            return theHandle != IntPtr.Zero ? new PageFooter(theHandle) : null;
        }
        public PageHeader InsertPageHeader()
        {
            IntPtr theHandle = RawAPI.GRReportInsertPageHeader(handle);
            return theHandle != IntPtr.Zero ? new PageHeader(theHandle) : null;
        }
        public void LoadBackImageFromFile(string PathFile)
        {
            RawAPI.GRReportLoadBackImageFromFile(handle, PathFile);
        }
        public bool LoadDataFromURL(string URL)
        {
            return RawAPI.GRReportLoadDataFromURL(handle, URL);
        }
        public bool LoadDataFromXML(string DataText)
        {
            return RawAPI.GRReportLoadDataFromXML(handle, DataText);
        }
        public bool LoadFromBinary(BinaryObject BinaryObject)
        {
            return RawAPI.GRReportLoadFromBinary(handle, BinaryObject.NativeHandle);
        }
        public bool LoadFromFile(string FileName)
        {
            return RawAPI.GRReportLoadFromFile(handle, FileName);
        }
        public bool LoadFromMemory(IntPtr pData, int ByteCount)
        {
            return RawAPI.GRReportLoadFromMemory(handle, pData, ByteCount);
        }
        public bool LoadFromStr(string Data)
        {
            return RawAPI.GRReportLoadFromStr(handle, Data);
        }
        public bool LoadFromURL(string URL)
        {
            return RawAPI.GRReportLoadFromURL(handle, URL);
        }
        public void LoadWatermarkFromFile(string PathFile)
        {
            RawAPI.GRReportLoadWatermarkFromFile(handle, PathFile);
        }
        public void LoopPrint()
        {
            RawAPI.GRReportLoopPrint(handle);
        }
        public Parameter ParameterByName(string Name)
        {
            IntPtr theHandle = RawAPI.GRReportParameterByName(handle, Name);
            return theHandle != IntPtr.Zero ? new Parameter(theHandle) : null;
        }
        public double PixelsToUnit(int Pixels)
        {
            return RawAPI.GRReportPixelsToUnit(handle, Pixels);
        }
        public ExportOption PrepareExport(ExportType ExportType)
        {
            IntPtr theHandle = RawAPI.GRReportPrepareExport(handle, ExportType);
            return theHandle != IntPtr.Zero ? new ExportOption(theHandle) : null;
        }
        public bool PrepareLoadData()
        {
            return RawAPI.GRReportPrepareLoadData(handle);
        }
        public void Print(bool ShowPrintDialog)
        {
            RawAPI.GRReportPrint(handle, ShowPrintDialog);
        }
        public bool PrintDocumentData(BinaryObject DocumentData)
        {
            return RawAPI.GRReportPrintDocumentData(handle, DocumentData.NativeHandle);
        }
        public void PrintEx(PrintGenerateStyle GenerateStyle, bool ShowPrintDialog)
        {
            RawAPI.GRReportPrintEx(handle, GenerateStyle, ShowPrintDialog);
        }
        public bool SaveToBinary(BinaryObject BinaryObject)
        {
            return RawAPI.GRReportSaveToBinary(handle, BinaryObject.NativeHandle);
        }
        public bool SaveToFile(string FileName)
        {
            return RawAPI.GRReportSaveToFile(handle, FileName);
        }
        public string SaveToStr()
        {
            return Marshal.PtrToStringUni(RawAPI.GRReportSaveToStr(handle));
        }
        public double SystemVarValue(SystemVarType SystemVar)
        {
            return RawAPI.GRReportSystemVarValue(handle, SystemVar);
        }
        public double SystemVarValue2(SystemVarType SystemVar, int GroupIndex)
        {
            return RawAPI.GRReportSystemVarValue2(handle, SystemVar, GroupIndex);
        }
        public double UnitToPixels(double UnitValue)
        {
            return RawAPI.GRReportUnitToPixels(handle, UnitValue);
        }
        public void UnprepareExport()
        {
            RawAPI.GRReportUnprepareExport(handle);
        }
        public int WebRegisterStatus()
        {
            return RawAPI.GRReportWebRegisterStatus(handle);
        }

        //<<event implement
        public delegate void _GeneralEventFun();
        public delegate void _IntArgEventFun(int val);
        public delegate void _StringArgEventFun(string str);
        public delegate void _RuntimeErrorEventFun(int errorID, string errorMsg);
        public delegate void _GroupEventFun(Group group);
        public delegate void _SectionEventFun(Section section);
        public delegate void _FieldEventFun(Field field);
        public delegate void _TextBoxEventFun(TextBox textbox);
        public delegate void _ControlCustomDrawEventFun(Control control, Graphics graphics);
        public delegate void _ChartEventFun(Chart chart);
        public delegate void _ExportEventFun(ExportOption option);

        public event _GeneralEventFun BeforePostRecord;
        public event _StringArgEventFun BeforeSort;
        public event _ChartEventFun ChartRequestData;
        public event _ControlCustomDrawEventFun ControlCustomDraw;
        public event _ExportEventFun ExportBegin;
        public event _ExportEventFun ExportEnd;
        public event _GeneralEventFun FetchRecord;
        public event _FieldEventFun FieldGetDisplayText;
        public event _GeneralEventFun GeneratePagesBegin;
        public event _GeneralEventFun GeneratePagesEnd;
        public event _GroupEventFun GroupBegin;
        public event _GroupEventFun GroupEnd;
        public event _GeneralEventFun Initialize; //public event _GeneralEventFun Initialize;
        public event _GeneralEventFun PageEnd;
        public event _GeneralEventFun PageProcessRecord;
        public event _GeneralEventFun PageStart;
        public event _GeneralEventFun PrintAborted;
        public event _GeneralEventFun PrintBegin;
        public event _GeneralEventFun PrintEnd;
        public event _IntArgEventFun PrintPage;
        public event _GeneralEventFun ProcessBegin;
        public event _GeneralEventFun ProcessEnd;
        public event _GeneralEventFun ProcessRecord;
        public event _RuntimeErrorEventFun RuntimeError;
        public event _SectionEventFun SectionFormat;
        public event _TextBoxEventFun TextBoxGetDisplayText;

        private void InitializeImpl(IntPtr reportHandle)
        {
            Initialize.Invoke();
        }
        private void FetchRecordImpl(IntPtr reportHandle)
        {
            FetchRecord.Invoke();
        }
        private void BeforePostRecordImpl(IntPtr reportHandle)
        {
            BeforePostRecord.Invoke();
        }
        private void BeforeSortImpl(IntPtr reportHandle, string str)
        {
            BeforeSort.Invoke(str);
        }
        private void RuntimeErrorImpl(IntPtr reportHandle, int errorID, string errorMsg)
        {
            RuntimeError.Invoke(errorID, errorMsg);
        }
        private void ProcessBeginImpl(IntPtr reportHandle)
        {
            ProcessBegin.Invoke();
        }
        private void ProcessEndImpl(IntPtr reportHandle)
        {
            ProcessEnd.Invoke();
        }
        private void GroupBeginImpl(IntPtr reportHandle, IntPtr groupHandle)
        {
            GroupBegin.Invoke(new Group(groupHandle));
        }
        private void GroupEndImpl(IntPtr reportHandle, IntPtr groupHandle)
        {
            GroupEnd.Invoke(new Group(groupHandle));
        }
        private void ProcessRecordImpl(IntPtr reportHandle)
        {
            ProcessRecord.Invoke();
        }
        private void GeneratePagesBeginImpl(IntPtr reportHandle)
        {
            GeneratePagesBegin.Invoke();
        }
        private void GeneratePagesEndImpl(IntPtr reportHandle)
        {
            GeneratePagesEnd.Invoke();
        }
        private void PageProcessRecordImpl(IntPtr reportHandle)
        {
            PageProcessRecord.Invoke();
        }
        private void PageStartImpl(IntPtr reportHandle)
        {
            PageStart.Invoke();
        }
        private void PageEndImpl(IntPtr reportHandle)
        {
            PageEnd.Invoke();
        }
        private void SectionFormatImpl(IntPtr reportHandle, IntPtr sectionHandle)
        {
            SectionFormat.Invoke(new Section(sectionHandle));
        }
        private void FieldGetDisplayTextImpl(IntPtr reportHandle, IntPtr fieldHandle)
        {
            FieldGetDisplayText.Invoke(new Field(fieldHandle));
        }
        private void TextBoxGetDisplayTextImpl(IntPtr reportHandle, IntPtr textBoxHandle)
        {
            TextBoxGetDisplayText.Invoke(new TextBox(textBoxHandle));
        }
        private void ControlCustomDrawImpl(IntPtr reportHandle, IntPtr controlHandle,
                 IntPtr graphicsHandle)
        {
            ControlCustomDraw.Invoke(new Control(controlHandle), new Graphics(graphicsHandle));
        }
        private void ChartRequestDataImpl(IntPtr reportHandle, IntPtr chartHandle)
        {
            ChartRequestData.Invoke(new Chart(chartHandle));
        }
        private void PrintBeginImpl(IntPtr reportHandle)
        {
            PrintBegin.Invoke();
        }
        private void PrintEndImpl(IntPtr reportHandle)
        {
            PrintEnd.Invoke();
        }
        private void PrintAbortedImpl(IntPtr reportHandle)
        {
            PrintAborted.Invoke();
        }
        private void PrintPageImpl(IntPtr reportHandle, int pageNo)
        {
            PrintPage.Invoke(pageNo);
        }
        private void ExportBeginImpl(IntPtr reportHandle, IntPtr optionHandle)
        {
            ExportBegin.Invoke(new ExportOption(optionHandle));
        }
        private void ExportEndImpl(IntPtr reportHandle, IntPtr optionHandle)
        {
            ExportEnd.Invoke(new ExportOption(optionHandle));
        }

        public void BindEvents()
        {
            if (BeforePostRecord != null && BeforePostRecord.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventBeforePostRecord2(NativeHandle, BeforePostRecordImpl);

            if (BeforeSort != null && BeforeSort.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventBeforeSort2(NativeHandle, BeforeSortImpl);

            if (ChartRequestData != null && ChartRequestData.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventChartRequestData2(NativeHandle, ChartRequestDataImpl);

            if (ControlCustomDraw != null && ControlCustomDraw.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventControlCustomDraw2(NativeHandle, ControlCustomDrawImpl);

            if (ExportBegin != null && ExportBegin.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventExportBegin2(NativeHandle, ExportBeginImpl);

            if (ExportEnd != null && ExportEnd.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventExportEnd2(NativeHandle, ExportEndImpl);

            if (FetchRecord != null && FetchRecord.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventFetchRecord2(NativeHandle, FetchRecordImpl);

            if (FieldGetDisplayText != null && FieldGetDisplayText.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventFieldGetDisplayText2(NativeHandle, FieldGetDisplayTextImpl);

            if (GeneratePagesBegin != null && GeneratePagesBegin.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventGeneratePagesBegin2(NativeHandle, GeneratePagesBeginImpl);

            if (GeneratePagesEnd != null && GeneratePagesEnd.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventGeneratePagesEnd2(NativeHandle, GeneratePagesEndImpl);

            if (GroupBegin != null && GroupBegin.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventGroupBegin2(NativeHandle, GroupBeginImpl);

            if (GroupEnd != null && GroupEnd.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventGroupEnd2(NativeHandle, GroupEndImpl);

            if (Initialize != null && Initialize.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventInitialize2(NativeHandle, InitializeImpl);

            if (PageEnd != null && PageEnd.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventPageEnd2(NativeHandle, PageEndImpl);

            if (PageProcessRecord != null && PageProcessRecord.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventPageProcessRecord2(NativeHandle, PageProcessRecordImpl);

            if (PageStart != null && PageStart.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventPageStart2(NativeHandle, PageStartImpl);

            if (PrintAborted != null && PrintAborted.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventPrintAborted2(NativeHandle, PrintAbortedImpl);

            if (PrintBegin != null && PrintBegin.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventPrintBegin2(NativeHandle, PrintBeginImpl);

            if (PrintEnd != null && PrintEnd.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventPrintEnd2(NativeHandle, PrintEndImpl);

            if (PrintPage != null && PrintPage.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventPrintPage2(NativeHandle, PrintPageImpl);

            if (ProcessBegin != null && ProcessBegin.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventProcessBegin2(NativeHandle, ProcessBeginImpl);

            if (ProcessEnd != null && ProcessEnd.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventProcessEnd2(NativeHandle, ProcessEndImpl);

            if (ProcessRecord != null && ProcessRecord.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventProcessRecord2(NativeHandle, ProcessRecordImpl);

            if (RuntimeError != null && RuntimeError.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventRuntimeError2(NativeHandle, RuntimeErrorImpl);

            if (SectionFormat != null && SectionFormat.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventSectionFormat2(NativeHandle, SectionFormatImpl);

            if (TextBoxGetDisplayText != null && TextBoxGetDisplayText.GetInvocationList().Length > 0)
                RawAPI.GRAttachEventTextBoxGetDisplayText2(NativeHandle, TextBoxGetDisplayTextImpl);
        }
        //>>event implement

        //static methos
        public static void InitConfig(string ModulePath)
        {
            RawAPI.GRInitConfig(ModulePath);
        }
        public static void SetConsoleMsgAsUTF8(bool Val)
        {
            RawAPI.GRSetConsoleMsgAsUTF8(Val);
        }
        public static bool IsModuleInitialized()
        {
            return RawAPI.GRIsModuleInitialized();
        }
        public static string GetModulePath()
        {
            return Marshal.PtrToStringUni(RawAPI.GRGetModulePath());
        }
        public static string GetModuleInfo()
        {
            return Marshal.PtrToStringUni(RawAPI.GRGetModuleInfo());
        }
        public static string GetModuleVersion()
        {
            return Marshal.PtrToStringUni(RawAPI.GRGetModuleVersion());
        }
        public static string GenerateBarcodeGraph(string BarcodeParams)
        {
            return Marshal.PtrToStringUni(RawAPI.GRGenerateBarcodeGraph(BarcodeParams));
        }
        public static IntPtr ReportCreate()
        {
            return RawAPI.GRReportCreate();
        }
        public static void ReportRelease(IntPtr Handle)
        {
            RawAPI.GRReportRelease(Handle);
        }

        static public void ConfigModulePath(string ModulePath)
        {
            _ModulePath = ModulePath;
            InitConfirm();
        }
        static private void InitConfirm()
        {
            if (!ReportLinux.IsModuleInitialized() && _ModulePath != null && _ModulePath != "")
            {
                InitConfig(_ModulePath);
                //_ModulePath = null; //这里不能清空，看来需要多次设置 
            }
        }
        static private string _ModulePath;

        public void Release()
        {
            if (selfCreated)
            {
                ReportRelease(handle);
            }
            _Free();
        }

    }

}