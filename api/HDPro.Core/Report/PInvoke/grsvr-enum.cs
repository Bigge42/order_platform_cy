//grsvr-enum.cs: 由锐浪报表接口维护工具自动生成，请勿手动修改
//生成时间: 2023/03/09 11:26:41


namespace gridreport
{

public enum AlignColumnStyle
{
    Left = 1, //对齐到列的左端。
    Right = 2, //对齐到列的右端。
    Both = 3, //对齐到列的左右两端，部件框宽度与列保持一致。
}

public enum AnchorStyles
{
    Left = 1, //该部件框锚定到其容器的左边缘。
    Top = 2, //该部件框锚定到其容器的上边缘。
    Right = 4, //该部件框锚定到其容器的右边缘。
    Bottom = 8, //该部件框锚定到其容器的下边缘。
}

public enum BackStyle
{
    Normal = 1, //部件框使用背景色填充他占据的矩形区域。
    Transparent = 2, //部件框背景透明，允许任何背景图像或其他部件框透过自身显示。
}

public enum BarcodeCaptionPosition
{
    None = 1, //条形码的文字不显示。
    Above = 2, //条形码的文字显示在上端。
    Below = 3, //条形码的文字显示在下端。
}

public enum BarcodeDirection
{
    LeftToRight = 1, //从左到右。
    RightToLeft = 2, //从右到左。
    TopToBottom = 3, //从上到下。
    BottomToTop = 4, //从下到上。
}

public enum BarcodeType
{
    Code25Intlv = 1, //Code25 Interleaved
    Code25Ind = 2, //Code25 Industrial,
    Code25Matrix = 3, //Code25 Matrix,
    Code39 = 4, //Code39
    Code39X = 5, //Code39X
    Code128A = 6, //Code128A
    Code128B = 7, //Code128B
    Code128C = 8, //Code128C
    Code93 = 9, //Code93
    Code93X = 10, //Code93 Extended
    CodeMSI = 11, //CodeMSI
    CodePostNet = 12, //CodePostNet
    CodeCodabar = 13, //CodeCodabar
    CodeEAN8 = 14, //CodeEAN8，商品码，数据只能为数字
    CodeEAN13 = 15, //CodeEAN13，，商品码，数据只能为数字。如果进行数据校验，提供12位数字数据，否则13位数字数据
    CodeUPC_A = 16, //CodeUPC_A，商品码，数据只能为数字
    CodeUPC_E0 = 17, //CodeUPC_E0，商品码，数据只能为数字
    CodeUPC_E1 = 18, //CodeUPC_E1，商品码，数据只能为数字
    CodeUPC_Supp2 = 19, //CodeUPC_Supp2
    CodeUPC_Supp5 = 20, //CodeUPC_Supp5
    CodeEAN128A = 21, //CodeEAN128A
    CodeEAN128B = 22, //CodeEAN128B
    CodeEAN128C = 23, //CodeEAN128C
    Code128 = 24, //Code128
    GS1Code128 = 25, //GS1 Code128(EAN128)
    ITF_14 = 26, //ITF_14
    PDF417 = 50, //二维码PDF417
    QRCode = 51, //二维码QRCode
    DataMatrix = 52, //二维码DataMatrix
    GS1DataMatrix = 53, //GS1 DataMatrix，以GS1标准编码数据生成 DataMatrix 码
    GS1QRCode = 54, //GS1 QRCode，以GS1标准编码数据生成 QRCode 码
}

public enum BorderStyles
{
    DrawLeft = 1, //部件框或明细网格显示左边边框线。
    DrawTop = 2, //部件框或明细网格显示上边边框线。
    DrawRight = 4, //部件框或明细网格显示右边边框线。
    DrawBottom = 8, //部件框或明细网格显示下边边框线。
}

public enum CenterStyle
{
    None = 0, //部件框在父容器中不居中对齐。
    Horizontal = 1, //部件框始终位于父容器水平方向的正中间。
    Vertical = 2, //部件框始终位于父容器垂直方向的正中间。
    Both = 3, //部件框始终位于父容器水平与垂直方向的正中间
}

public enum ChartType
{
    BarChart = 1, //柱图。
    PieChart = 2, //饼图。
    LineChart = 3, //折线图。
    StackedBarChart = 4, //叠加柱图
    XYScatterChart = 5, //散列点图
    XYLineChart = 6, //散列点连线图
    CurveLineChart = 7, //连曲线图
    XYCurveLineChart = 8, //散列点连曲线图
    Bubble = 9, //气泡图
    StackedBar100Chart = 10, //百分比柱状图
    ColumnChart = 11, //横向柱状图
    StackedColumnChart = 12, //横向叠加柱状图
    StackedColumn100Chart = 13, //横向百分比柱状图
}

public enum ChartVarType
{
    XVal = 1, //X值
    YVal = 2, //Y值
    ZVal = 3, //Z值
    YVal100ByGroup = 4, //Y值在当前组簇中的百分比
    YVal100BySeries = 5, //Y值在当前序列中的百分比
    YValTotalByGroup = 6, //当前组簇中的Y值合计
    YValTotalBySeries = 7, //当前序列中的Y值合计
    SeriesLabel = 8, //当前序列中的标签文字
    GroupLabel = 9, //当前组簇中的标签文字
}

public enum CollateKind
{
    Default = 1, //由打印机确定是否逐份打印
    Collate = 2, //逐份打印
    NotCollate = 3, //非逐份打印
}

public enum ColumnPrintAdaptMethod
{
    Discard = 1, //在打印输出时，超出页面输出范围的列内容将被忽略掉，即不输出显示。
    Wrap = 2, //在打印输出时，超出页面输出范围的列内容将另起新行输出显示。
    ResizeToFit = 3, //在打印输出时，根据列的宽度按比列将所有要输出的列的总宽度调整到页面输出区域的宽度。
    ShrinkToFit = 4, //在打印输出时，如果列的总宽度超出页面输出范围，与grcpamResizeToFit相同，反之按设计时宽度输出。
    ToNewPage = 5, //在打印输出时，超出页面输出范围的列内容将另起新页输出显示，按先从上到下的顺序输出。
    ToNewPageEx = 6, //在打印输出时，超出页面输出范围的列内容将另起新页输出显示，按先从左到右的顺序输出。
    ToNewPageRFC = 7, //在打印输出时，超出页面输出范围的列内容将另起新页输出显示，且左边的固定列在每页中重复输出，按先从上到下的顺序输出。
    ToNewPageRFCEx = 8, //在打印输出时，超出页面输出范围的列内容将另起新页输出显示，且左边的固定列在每页中重复输出，按先从左到右的顺序输出。
    WrapExcludeGroup = 9, //类同 grcpamWrap，但分组头尾不另起新行。
}

public enum ControlType
{
    StaticBox = 1, //静态文字框。
    ShapeBox = 2, //图形框。
    SystemVarBox = 3, //系统变量框。
    FieldBox = 4, //字段框。
    SummaryBox = 5, //统计框。
    RichTextBox = 6, //RTF格式文字框
    PictureBox = 7, //图像框。
    MemoBox = 8, //综合文字框。
    SubReport = 9, //子报表。
    Line = 10, //线段。
    Chart = 11, //图表。
    Barcode = 12, //条形码。
    FreeGrid = 13, //自由表格。
}

public enum DisplayCursor
{
    Default = 0, //默认光标。
    Arrow = 1, //普通箭头光标。
    Magnify = 2, //方大镜光标。
    Finger = 3, //手指光标。
    Affirm = 4, //肯定光标。
    Negative = 5, //否定光标。
}

public enum DockStyle
{
    None = 0, //该部件框未停靠。
    Left = 1, //该部件框的左边缘停靠在父容器的左边缘。
    Top = 2, //该部件框的上边缘停靠在父容器的顶端。
    Right = 3, //该部件框的右边缘停靠在父容器的右边缘。
    Bottom = 4, //该部件框的下边缘停靠在父容器的底部。
    Fill = 5, //部件框的各个边缘分别停靠在父容器的各个边缘，并且适当调整大小。
}

public enum DocType
{
    Object = 1, //按对象方式保存模板数据，此种格式为 Grid++Report 自有格式，内容非常直观易理解。
    JSON = 2, //按 JSON 格式保存报表模板数据，完全符合 JSON 规范要求。
    Register = 3, //报表模板的WEB报表注册加密格式。
}

public enum DrawRotation
{
    Rotate0 = 0, //不旋转。
    Rotate90 = 1, //旋转90度。
    Rotate180 = 2, //旋转180度。
    Rotate270 = 3, //旋转270度。
}

public enum DtmxEncoding
{
    Auto = 2, //由程序根据数据自动选择编码方式
    Ascii = 3, //Ascii编码方式
    C40 = 4, //C40编码方式
    Text = 5, //Text编码方式
    X12 = 6, //X12编码方式
    Edifact = 7, //Edifact编码方式
    Base256 = 8, //Base256编码方式
}

public enum DtmxMatrixSize
{
    Auto = 2, //由程序根据数据量自动选择
    _10x10 = 4,
    _12x12 = 5,
    _14x14 = 6,
    _16x16 = 7,
    _18x18 = 8,
    _20x20 = 9,
    _22x22 = 10,
    _24x24 = 11,
    _26x26 = 12,
    _32x32 = 13,
    _36x36 = 14,
    _40x40 = 15,
    _44x44 = 16,
    _48x48 = 17,
    _52x52 = 18,
    _64x64 = 19,
    _72x72 = 20,
    _80x80 = 21,
    _88x88 = 22,
    _96x96 = 23,
    _104x104 = 24,
    _120x120 = 25,
    _132x132 = 26,
    _144x144 = 27,
    _8x18 = 28,
    _8x32 = 29,
    _12x26 = 30,
    _12x36 = 31,
    _16x36 = 32,
    _16x48 = 33,
}

public enum DuplexKind
{
    Default = 0, //打印机默认的双面打印设置。
    Simplex = 1, //单面打印。
    Vertical = 2, //双面垂直打印。
    Horizontal = 3, //双面水平打印。
}

public enum FieldType
{
    String = 1, //字符字段。
    Integer = 2, //整数字段。
    Float = 3, //浮点数字段。
    Currency = 4, //货币字段。
    Boolean = 5, //布尔字段。
    DateTime = 6, //日期时间字段。
    Binary = 7, //二进制字段。
}

public enum GradientMode
{
    None = 0, //不应用渐变。 
    Center = 1, //渐变是从中心向外应用的。 
    LeftRight = 2, //渐变是从左向右应用的。
    TopBottom = 3, //渐变是从上到下应用的。 
    HorizontalCenter = 4, //渐变是沿水平方向从中心向外应用的。 
    VerticalCenter = 5, //渐变是沿垂直方向从中心向外应用的。 
    DiagonalLeft = 6, //渐变是沿对角方向从左向右应用的。
    DiagonalRight = 7, //渐变是沿对角方向从右向左应用的。
}

public enum GrpKpTogetherStyle
{
    None = 1, //不要求分组头行与本分组项其他行打印输出聚集在相同页。
    FirstDetail = 2, //要求分组头行与本分组项的第一个明细记录行打印输出在相同页。
    All = 3, //要求分组头行与本分组项的其他所有行尽量打印输出在相同页。
}

public enum LockType
{
    None = 0, //不锁定
    Inherited = 1, //继承：从父级对象继承锁定设置状态
    Layout = 2, //布局：不能修改布局相关的属性设置，“布局”类别的属性不允许修改
    Object = 3, //对象：对象本身不允许删除，不允许增加或删除其子对象
    Data = 4, //数据：在 Object 限定基础上，数据：不能修改数据相关的属性设置，“数据”类别的属性不允许修改
    DataAction = 5, //数据行为：在 Data 限定基础上，不能修改“行为”相关的属性设置，“行为”与“脚本”类别的属性不允许修改
    All = 10, //全部：任何修改都不允许，包括外观与布局相关设置都不允许改变
}

public enum NewPageColumnStyle
{
    None = 1, //分组头在任何时候都不另起新页栏。
    Before = 2, //分组头在打印输出之前另起新页栏进行输出。
    After = 3, //分组头在打印输出之后另起新页栏。后续内容输出在新页栏中。
    BeforeAfter = 4, //分组头在打印输出之前与之后都另起新页栏。
}

public enum NewPageStyle
{
    None = 1, //报表节不强制产生新页。
    Before = 2, //报表节在打印输出之前要求产生新页，保证本节在新页中输出。
    After = 3, //报表节在打印输出之后要求产生新页，保证本节之后的后续节在新页中输出。
    BeforeAfter = 4, //报表节在打印输出之前与之后要求产生新页。
}

public enum OCGroupHeaderVAlign
{
    Top = 1, //占列式分组头显示在整个分组列区域的顶部。
    Middle = 2, //占列式分组头显示在整个分组列区域的中部（垂直方向）。
    Bottom = 3, //占列式分组头显示在整个分组列区域的底部。
}

public enum PageColumnDirection
{
    DownAcross = 1, //按从上到下，再从左到右的顺序在页栏中打印输出。
    AcrossDown = 2, //按从左到右，再从上到下的顺序在页栏中打印输出。
    DownAcrossEqual = 3, //按从上到下，再从左到右的顺序在页栏中打印输出，保持每栏输出基本一样多的数据。
}

public enum PaperKind
{
    Letter = 1, //1/2- by 11-inches Letter，215.9 x 279.4 毫米。 
    LetterSmall = 2, //Letter Small, 8 1/2- by 11-inches 215.9 x 279.4 毫米。 
    Tabloid = 3, //11- by 17-inches Tabloid，279.4 x 431.8 毫米。 
    Ledger = 4, //17- by 11-inches 431.8 x 279.4 毫米。 
    Legal = 5, //1/2- by 14-inches Legal，215.9 x 355.6 毫米。 
    Statement = 6, //5 1/2- by 8 1/2-inches 139.7 x 215.9 毫米。 
    Executive = 7, //7 1/4- by 10 1/2-inches 184.1 x 266.7 毫米。 
    A3 = 8, //A3，297- by 420-millimeters 
    A4 = 9, //A4，210- by 297-millimeters 
    A4Small = 10, //A4 Small， 210- by 297-millimeters
    A5 = 11, //148- by 210-millimeters 
    B4 = 12, //250- by 354-millimeters 
    B5 = 13, //182- by 257-millimeter paper 
    Folio = 14, //8 1/2- by 13-inch paper 215.9 x 330.2 毫米。 
    Quarto = 15, //215- by 275-millimeter paper 
    _10X14 = 16, //by 14-inch sheet Paper10x14 纸张大小为 254 x 355.6 毫米。 
    _11X17 = 17, //17-inch sheet 纸张大小为 279.4 x 431.8 毫米。
    Note = 18, //8 1/2- by 11-inches Note，215.9 x 279.4 毫米。 
    Envelope9 = 19, //3 7/8- by 8 7/8-inches envelope（229 × 324 毫米）
    Envelope10 = 20, //4 1/8- by 9 1/2-inches #10 Envelope，104.8 x 241.3 毫米。 
    Envelope11 = 21, //4 1/2- by 10 3/8-inches #11 Envelope，114.3 x 263.5 毫米。 
    Envelope12 = 22, //4 3/4- by 11-inches #12 Envelope，120.7 x 279.4 毫米。 
    Envelope14 = 23, //5- by 11 1/2-inches #14 Envelope，127 x 292.1 毫米。 
    CSheet = 24, //17- by 22-inches C 型纸，431.8 x 558.8 毫米。 
    DSheet = 25, //22- by 34-inches D 型纸，558.8 x 863.6 毫米。  
    ESheet = 26, //E Sheet 34- by 44-inches 863.6 x 1117.6 毫米。 
    EnvelopeDL = 27, //110- by 220-millimeters 
    EnvelopeC5 = 28, //162- by 229-millimeters C5 Envelope，162 x 229 毫米。 
    EnvelopeC3 = 29, //324- by 458-millimeters C3 Envelope，324 x 458 毫米。 
    EnvelopeC4 = 30, //229- by 324-millimeters 
    EnvelopeC6 = 31, //114- by 162-millimeters 
    EnvelopeC65 = 32, //114- by 229-millimeters 
    EnvelopeB4 = 33, //250- by 353-millimeters 
    EnvelopeB5 = 34, //176- by 250-millimeters 
    EnvelopeB6 = 35, //176- by 125-millimeters 
    EnvelopeItaly = 36, //110- by 230-millimeters 110 x 230 毫米。 
    EnvelopeMonarch = 37, //3 7/8- by 7 1/2-inches 98.4 x 190.5 毫米。 
    EnvelopePersonal = 38, //3 5/8- by 6 1/2-inches 92.1 x 165.1 毫米。 
    Fanfold = 39, //14 7/8- by 11-inches 377.8 x 279.4 毫米。 
    A6 = 70, //105- by 148-millimeters 
    B6 = 88, //128- by 182-millimeters 
    _12X11 = 90, //11-inch sheet Standard 纸（305 × 279 毫米）
    P16K = 93, //PRC 16K, 146- by 215-millimeters 
    P32K = 94, //PRC 32K, 97- by 151-millimeters 
    Default = 255, //当前打印机的默认默认纸张
    Custom = 256, //用户自定义纸张，需要指定纸张的宽度与长度属性
}

public enum PaperOrientation
{
    Default = 0, //应用打印机当前设置的纸张输出方向
    Portrait = 1, //纸张输出方向为纵向。
    Landscape = 2, //纸张输出方向为横向。
}

public enum PaperSourceKind
{
    Default = 0, //打印机默认。
    Upper = 1, //打印机的上层纸盒（如果打印机只有一个纸盒，那么这个纸盒就是上层纸盒）。
    Lower = 2, //打印机的下层纸盒。
    Middle = 3, //打印机的中层纸盒。
    Manual = 4, //以手动方式送入的纸张。
    Envelope = 5, //信封。
    ManualFeed = 6, //以手动方式送入的信封。
    AutomaticFeed = 7, //自动送入的纸张。
    TractorFeed = 8, //牵引器送纸。
    SmallFormat = 9, //小纸。
    LargeFormat = 10, //大纸。
    LargeCapacity = 11, //打印机的大容量纸盒。
    Cassette = 14, //卡式纸盒。
    FormSource = 15, //打印机的默认送纸盒。
    Custom = 256, //打印机特定的纸张来源。
}

public enum ParameterDataType
{
    String = 1, //字符串类型，可以为任意长度。
    Integer = 2, //整数类型，可以设定格式化串。
    Float = 3, //浮点数类型，可以设定格式化串。
    Boolean = 5, //布尔类型，可以真值与假值的显示文字。
    DateTime = 6, //日期时间类型，可以设定格式化串。
}

public enum PenStyle
{
    Solid = 0, //画实线。
    Dash = 1, //画段虚线。
    Dot = 2, //画点虚线。
    DashDot = 3, //画点段虚线。
    DashDotDot = 4, //画双点段虚线。
}

public enum PeriodType
{
    None = 0, //不确定期间。
    Day = 1, //日
    Week = 2, //周
    ThirdMonth = 3, //旬
    HalfMonth = 4, //半月
    Month = 5, //月
    Quarter = 6, //季
    HalfYear = 7, //半年
    Year = 8, //年
    Calendar = 9, //日历
}

public enum PictureAlignment
{
    TopLeft = 1, //图像的左上角紧靠图像框的左上角显示。
    TopRight = 2, //图像的右上角紧靠图像框的右上角显示。
    Center = 3, //图像居中显示在图像框中。
    BottomLeft = 4, //图像的左下角紧靠图像框的左下角显示。
    BottomRight = 5, //图像的右下角紧靠图像框的右下角显示。
}

public enum PictureRotateMode
{
    None = 0, //不旋转
    Left = 1, //左旋
    Right = 2, //右旋
    Flip = 3, //颠倒
    Mirror = 4, //镜像
}

public enum PictureSizeMode
{
    Clip = 1, //图像不进行缩放，按原始尺寸显示。
    Stretch = 2, //图像伸缩到完全显示到图像框中。
    Zoom = 3, //根据图像框的大小图像保持宽高比例伸缩至某一方向完全铺满。
    EnsureFullView = 4, //如果图像不能在图像框中完全显示，根据图像框的大小图像保持宽高比例伸缩至某一方向完全铺满。反之图像保持原始尺寸显示。
    Tile = 5, //平铺多次显示图像，铺满整个显示区域。
}

public enum PictureTransparentMode
{
    None = 0, //不透明
    Overlying = 1, //叠加透明
    Background = 2, //背景透明
}

public enum PictureType
{
    Unknown = 0, //未知
    BMP = 1, //BMP
    GIF = 2, //GIF
    JPEG = 3, //JPEG
    PNG = 4, //PNG
    ICON = 5, //ICON
    TIFF = 6, //TIFF
    WMF = 10, //WMF
}

public enum PointMarkerStyle
{
    None = -1, //无图案
    Square = 0, //框
    SquareCheck = 1, //框加勾
    SquareCross = 2, //框加叉
    Circle = 3, //圈
    CirclePoint = 4, //圈加点
    CircleCross = 5, //圈加叉
    Dimond = 6, //钻石
    Triangle = 7, //三角形
    Cross = 8, //叉
    Cross4 = 9, //米字叉
    Star4 = 10, //4角星
    Star5 = 11, //5角星
    Star6 = 12, //6角星
    Star10 = 13, //10角星
}

public enum PrintGenerateStyle
{
    OnlyForm = 1, //只生成报表表单数据。
    OnlyContent = 2, //只生成报表内容数据。
    All = 3, //生成报表所有数据。
    PreviewAll = 4, //预览报表全部内容，但只打印出内容数据。
}

public enum PrintPageType
{
    AllSelectionPages = 1, //输出选定页范围内的所有页。
    OddSelectionPages = 2, //输出选定页范围内的奇数页。
    EvenSelectionPages = 3, //输出选定页范围内的偶数页。
}

public enum PrintRangeType
{
    AllPages = 1, //选定全部页。
    CurrentPage = 2, //选定当前页，只有在从打印御览状态中执行打印任务时才有效。
    SelectionPages = 3, //指定的页范围。
}

public enum PrintType
{
    Form = 1, //表单固定数据，在套打输出内容时不输出。
    Content = 2, //表单内容数据，在套打输出内容时输出。
}

public enum RepeatStyle
{
    None = 1, //明细网格标题不重复输出。
    OnPage = 2, //明细网格标题在每页重复输出。
    OnPageColumn = 4, //明细网格标题在每个页栏重复输出。
    OnGroupHeader = 8, //明细网格标题在最内层的分组头之后重复输出。
    OnGroupHeaderPage = 10, //明细网格标题在每页与最内层的分组头之后重复输出。
}

public enum ReportDisplayMode
{
    ScreenView = 1, //正在查询显示器中报表处于自画过程中。
    PrintGenerate = 2, //报表处于打印页面生成过程中。
    Idle = 3, //报表不处于任何生成显示过程中。
}

public enum ScriptType
{
    JScript = 1, //JScript 脚本语言。
    VBScript = 2, //VBScript 脚本语言。
}

public enum SectionType
{
    PageHeader = 1, //页眉。
    ReportHeader = 2, //报表头。
    ReportFooter = 3, //报表尾。
    PageFooter = 4, //页脚。
    ColumnTitle = 5, //明细网格标题行。
    ColumnContent = 6, //明细网格内容行。
    GroupHeader = 7, //分组头。
    GroupFooter = 8, //分组尾。
    FreeGridRow = 9, //自由表格行。
}

public enum ShapeType
{
    Circle = 1, //圆。
    Ellipse = 2, //椭圆。
    Rectangle = 3, //矩形。
    RoundRect = 4, //圆角矩形。
    RoundSquare = 5, //圆角正方形。
    Square = 6, //正方形。
    Line = 7, //直线。
}

public enum SharePrintSetupOption
{
    PaperMargin = 1, //页边距项目，对应上、下、左，右页边距属性。
    PaperKind = 2, //纸张类型项目，对应纸张类型与大小属性。
    PaperOrientation = 4, //纸张的打印方向项目。
    PaperSource = 8, //纸张的进纸来源项目。
    SelectedPrinter = 16, //当前选定的打印机项目，对应打印机名称属性。
}

public enum SheetPages
{
    _1Pages = 1, //指定版面数为1，也就是最常规的单版面输出方式
    _2Pages = 2, //指定版面数为2
    _4Pages = 3, //指定版面数为4
    _6Pages = 4, //指定版面数为6
    _8Pages = 5, //指定版面数为8
    _16Pages = 6, //指定版面数为16
}

public enum ShiftMode
{
    Never = 1, //不进行位移。
    Always = 2, //总是进行位移。
    WhenOverLapped = 3, //只有与上部部件框在垂直方向有重叠时才进行位移。
}

public enum StorageFormat
{
    Text = 1, //文本格式。
    Binary = 2, //二进制格式。
    BinBase64 = 3, //Base64编码格式，Base64编码是用可见的ASCII字符表示二进制数据。
}

public enum StringAlignment
{
    Near = 1, //指定文本靠近布局对齐。在左到右布局中，近端位置是左。在右到左布局中，近端位置是右。
    Center = 2, //指定文本在布局矩形中居中对齐。
    Far = 3, //指定文本远离布局矩形的原点位置对齐。在左到右布局中，远端位置是右。在右到左布局中，远端位置是左。
}

public enum SummaryFun
{
    Sum = 1, //统计某个字段的合计值。
    Avg = 2, //统计某个字段的平均值。
    Count = 3, //统计明细记录的个数。
    Min = 4, //找出某个字段的最小值。
    Max = 5, //找出某个字段的最大值。
    Var = 6, //方差
    VarP = 7, //总体方差
    StdDev = 8, //标准偏差
    StdDevP = 9, //总体标准偏差
    AveDev = 10, //平均偏差
    DevSq = 11, //偏差平方和
    CountBlank = 12, //空值个数
    CountA = 13, //非空值个数
    Distinct = 14, //非重复值个数
    AvgA = 15, //非空值平均
    MinN = 16, //第N个最小值
    MaxN = 17, //第N个最大值
    StrMin = 18, //字符最小值
    StrMax = 19, //字符最大值
    VarA = 20, //非空方差
    VarPA = 21, //非空总体方差
    StdDevA = 22, //非空标准偏差
    StdDevPA = 23, //非空总体标准偏差
    AveDevA = 24, //非空平均偏差
    DevSqA = 25, //非空偏差平方和
    SumAbs = 26, //绝对值合计
    SumAcc = 27, //全程累计，从开始到当前行的合计
    GroupSumAcc = 28, //组累计，在上级组内累计，开始一个新分组，重新累计
    Join = 29, //拼接，将数据显示文字拼接在一起。各个数据项之间的分隔符号由“格式”值确定。如果要换行，在“格式”加入"\n"字符。如果是统计框，指定
    JoinUnique = 30, //非重复拼接，重复项只拼接一次。其它类同“拼接”函数
}

public enum SystemImage
{
    Radio3DUnchecked = -10, //值为-10，3D形式的无线按钮(Radio)非选中标志。
    Radio3DChecked = -9, //值为-9，3D形式的无线按钮(Radio)选中标志。
    ArrowUp = -8, //值为-8，朝上箭头标志。
    ArrowDown = -7, //值为-7，朝下箭头标志。
    Negative = -6, //值为-6，否定标志。
    Affirm = -5, //值为-5，肯定标志。
    _3DUnchecked = -4, //值为-4，3D形式的非选中标志。
    _3DChecked = -3, //值为-3，3D形式的选中标志。
    Unchecked = -2, //值为-2，非选中标志。
    Checked = -1, //值为-1，选中标志。
}

public enum SystemVarType
{
    CurrentDateTime = 1, //计算机的当前日期时间。
    PageCount = 2, //总页数。
    PageNumber = 3, //当前页号。
    RecordNo = 4, //明细记录的当前记录号，从1开始计数。
    RowNo = 8, //明细网格的当前行号，从1开始计数。
    RecordCount = 19, //明细记录的记录数。
    GroupNo = 20, //分组序号，某个分组的序号，与分组项个数关联，序号从1开始
    GroupCount = 21, //分组数，某个分组产生的分组项个数（全程变量，全程统一值）
    GroupRowNo = 22, //分组项行号，在一个分组内重启序号，序号从1开始
    GroupRowCount = 23, //分组项行数，某个分组项包含的明细记录(行)数。
    GroupPageNo = 24, //分组项页号
    GroupPageCount = 25, //分组项页数
}

public enum TextAlign
{
    TopLeft = 17, //内容在垂直方向上顶部对齐，在水平方向上左边对齐。
    TopCenter = 18, //内容在垂直方向上顶部对齐，在水平方向上居中对齐。
    TopRight = 20, //内容在垂直方向上顶部对齐，在水平方向上右边对齐。
    MiddleLeft = 33, //内容在垂直方向上中间对齐，在水平方向上左边对齐。
    MiddleCenter = 34, //内容在垂直方向上中间对齐，在水平方向上居中对齐。
    MiddleRight = 36, //内容在垂直方向上中间对齐，在水平方向上右边对齐。
    BottomLeft = 65, //内容在垂直方向上底边对齐，在水平方向上左边对齐。
    BottomCenter = 66, //内容在垂直方向上底边对齐，在水平方向上居中对齐。
    BottomRight = 68, //内容在垂直方向上底边对齐，在水平方向上右边对齐。
    TopJustiy = 144, //内容在垂直方向上顶部对齐，在水平方向上两端分散对齐。
    MiddleJustiy = 160, //内容在垂直方向上中间对齐，在水平方向上两端分散对齐。
    BottomJustiy = 192, //内容在垂直方向上底边对齐，在水平方向上两端分散对齐。
}

public enum TextEncodeMode
{
    Ansi = 1, //ANSI编码
    UTF8 = 2, //UTF-8编码，数据最前面有2个字节的标识数据。
    Unicode = 3, //Unicode编码
    UTF8Pure = 4, //UTF-8编码，数据最前面没有标识
    UTF8WithHead = 2, //此项同grtemUTF8，特别注意：枚举值也应为 2
}

public enum TextOrientation
{
    Default = 0, //默认方式，按从左到右，从上到下方式显示文字。
    U2DL2R0 = 5, //按从上到下，从左到右方式显示文字，文字不旋转。
    U2DR2L0 = 9, //按从上到下，从右到左方式显示文字，文字不旋转。
    D2UL2R1 = 22, //按从下到上，从左到右方式显示文字，文字旋转。
    U2DR2L1 = 25, //按从上到下，从右到左方式显示文字，文字旋转。
    L2RD2U0 = 38, //按从左到右，从下到上方式显示文字，适合用来进行脊背打印，字体应该选择旋转的，即字体名称前带“@”符号。
    L2RD2U1 = 54, //按从左到右，从下到上方式显示文字，文字旋转，适合用来进行脊背打印。
    Invert = 58, //倒影方式。
}

public enum Unit
{
    Cm = 1, //以厘米为单位表示报表部件的尺寸与位置。
    Inch = 2, //以英寸为单位表示报表部件的尺寸与位置。
}

public enum ExportImageType
{
    BMP = 1, //Bitmap 位图图像格式。
    PNG = 2, //PNG 图像格式。
    JPEG = 3, //JPEG 图像格式。
    TIFF = 5, //TIFF 图像格式。
}

public enum ExportType
{
    XLS = 1, //导出Excel文件。
    TXT = 2, //导出文本文件。
    HTM = 3, //导出Html超文本文件。
    RTF = 4, //导出RTF文件。
    PDF = 5, //导出PDF格式文件。
    CSV = 6, //导出CSV格式文件。
    IMG = 7, //导出图像文件，支持多种图像格式。
}

public enum PDFDocPermission
{
    Print = 1, //是否允许打印。
    EditAll = 2, //是否允许完整修改文档。
    Copy = 3, //是否允许复制文档内容。
    Edit = 4, //是否允许修改文档。
}

}