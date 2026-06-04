SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.Sys_Menu', N'U') IS NULL
BEGIN
    PRINT N'错误：Sys_Menu 表不存在，无法新增计划修改看板菜单。';
    RETURN;
END;
GO

DECLARE @ParentId INT;
DECLARE @SourceMenuId INT;
DECLARE @NewMenuId INT;
DECLARE @Auth NVARCHAR(MAX);
DECLARE @Icon NVARCHAR(50);
DECLARE @OrderNo INT;
DECLARE @AuthData INT;
DECLARE @LinkType INT;
DECLARE @Enable TINYINT;
DECLARE @MenuType INT;

SELECT TOP (1)
    @ParentId = Menu_Id
FROM dbo.Sys_Menu
WHERE MenuName = N'计划管理'
  AND ISNULL(MenuType, 0) = 0
ORDER BY ISNULL(OrderNo, 0) DESC, Menu_Id;

IF @ParentId IS NULL
BEGIN
    PRINT N'错误：未找到“计划管理”菜单，请先确认父级菜单名称。';
    RETURN;
END;

SELECT TOP (1)
    @SourceMenuId = Menu_Id,
    @Auth = Auth,
    @Icon = Icon,
    @OrderNo = OrderNo,
    @AuthData = AuthData,
    @LinkType = LinkType,
    @Enable = Enable,
    @MenuType = MenuType
FROM dbo.Sys_Menu
WHERE Url = N'/WZ_OrderCycleBase'
   OR TableName = N'WZ_OrderCycleBase'
   OR MenuName = N'排产智能体优化看板'
ORDER BY
    CASE WHEN Url = N'/WZ_OrderCycleBase' THEN 0 ELSE 1 END,
    Menu_Id;

SET @Auth = COALESCE(NULLIF(@Auth, N''), N'[
  {"text":"查询","value":"Search"},
  {"text":"新建","value":"Add"},
  {"text":"编辑","value":"Update"},
  {"text":"删除","value":"Delete"},
  {"text":"导出","value":"Export"}
]');
SET @Icon = COALESCE(NULLIF(@Icon, N''), N'el-icon-date');
SET @OrderNo = COALESCE(
    @OrderNo,
    (SELECT ISNULL(MAX(OrderNo), 0) + 1 FROM dbo.Sys_Menu WHERE ParentId = @ParentId)
);
SET @AuthData = COALESCE(@AuthData, 0);
SET @LinkType = COALESCE(@LinkType, 0);
SET @Enable = COALESCE(@Enable, 1);
SET @MenuType = COALESCE(@MenuType, 0);

SELECT TOP (1)
    @NewMenuId = Menu_Id
FROM dbo.Sys_Menu
WHERE Url = N'/WZ_OrderCyclePlanModifyBoard'
   OR TableName = N'WZ_OrderCyclePlanModifyBoard'
   OR MenuName = N'计划修改看板'
ORDER BY Menu_Id;

IF @NewMenuId IS NULL
BEGIN
    INSERT INTO dbo.Sys_Menu
    (
        ParentId,
        MenuName,
        TableName,
        Url,
        Auth,
        AuthData,
        LinkType,
        Description,
        Icon,
        OrderNo,
        Creator,
        CreateDate,
        Enable,
        MenuType
    )
    VALUES
    (
        @ParentId,
        N'计划修改看板',
        N'WZ_OrderCyclePlanModifyBoard',
        N'/WZ_OrderCyclePlanModifyBoard',
        @Auth,
        @AuthData,
        @LinkType,
        N'参考排产智能体优化看板字段顺序，按销售订单号和计划跟踪号开头字符查询。',
        @Icon,
        @OrderNo,
        N'Codex',
        GETDATE(),
        @Enable,
        @MenuType
    );

    SET @NewMenuId = SCOPE_IDENTITY();
    PRINT N'已新增“计划修改看板”菜单。';
END
ELSE
BEGIN
    UPDATE dbo.Sys_Menu
    SET ParentId = @ParentId,
        MenuName = N'计划修改看板',
        TableName = N'WZ_OrderCyclePlanModifyBoard',
        Url = N'/WZ_OrderCyclePlanModifyBoard',
        Auth = CASE WHEN NULLIF(Auth, N'') IS NULL THEN @Auth ELSE Auth END,
        AuthData = COALESCE(AuthData, @AuthData),
        LinkType = COALESCE(LinkType, @LinkType),
        Icon = COALESCE(NULLIF(Icon, N''), @Icon),
        OrderNo = COALESCE(OrderNo, @OrderNo),
        Enable = COALESCE(Enable, @Enable),
        MenuType = COALESCE(MenuType, @MenuType),
        Modifier = N'Codex',
        ModifyDate = GETDATE()
    WHERE Menu_Id = @NewMenuId;

    PRINT N'“计划修改看板”菜单已存在，已校准路由与父级。';
END;

IF @SourceMenuId IS NOT NULL
   AND OBJECT_ID(N'dbo.Sys_RoleAuth', N'U') IS NOT NULL
BEGIN
    INSERT INTO dbo.Sys_RoleAuth
    (
        Role_Id,
        User_Id,
        Menu_Id,
        AuthValue,
        AuthMenuData,
        Creator,
        CreateDate
    )
    SELECT
        src.Role_Id,
        src.User_Id,
        @NewMenuId,
        src.AuthValue,
        src.AuthMenuData,
        N'Codex',
        GETDATE()
    FROM dbo.Sys_RoleAuth AS src
    WHERE src.Menu_Id = @SourceMenuId
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.Sys_RoleAuth AS dst
          WHERE dst.Menu_Id = @NewMenuId
            AND ISNULL(dst.Role_Id, -1) = ISNULL(src.Role_Id, -1)
            AND ISNULL(dst.User_Id, -1) = ISNULL(src.User_Id, -1)
      );

    PRINT N'已复制原排产看板角色授权到计划修改看板。';
END;
GO
