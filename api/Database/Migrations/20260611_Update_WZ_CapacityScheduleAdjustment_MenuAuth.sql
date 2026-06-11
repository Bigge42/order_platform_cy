SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.Sys_Menu', N'U') IS NULL
BEGIN
    PRINT N'错误：Sys_Menu 表不存在，无法校准异常排产调整工作台菜单。';
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
DECLARE @TargetAuthValue NVARCHAR(1000) = N'Search,Update';

SELECT TOP (1)
    @ParentId = Menu_Id
FROM dbo.Sys_Menu
WHERE MenuName = N'计划管理'
  AND ISNULL(MenuType, 0) = 0
ORDER BY ISNULL(OrderNo, 0) DESC, Menu_Id;

IF @ParentId IS NULL
BEGIN
    PRINT N'错误：未找到“计划管理”菜单，无法校准异常排产调整工作台位置。';
    RETURN;
END;

SELECT TOP (1)
    @SourceMenuId = Menu_Id,
    @Auth = Auth,
    @Icon = Icon,
    @OrderNo = CASE WHEN ParentId = @ParentId THEN ISNULL(OrderNo, 0) + 1 ELSE NULL END,
    @AuthData = AuthData,
    @LinkType = LinkType
FROM dbo.Sys_Menu
WHERE Url = N'/WZ_OrderCycleBase'
   OR TableName = N'WZ_OrderCycleBase'
   OR MenuName = N'排产智能体优化看板'
ORDER BY
    CASE WHEN Url = N'/WZ_OrderCycleBase' THEN 0 ELSE 1 END,
    Menu_Id;

SET @Auth = COALESCE(NULLIF(@Auth, N''), N'[
  {"text":"查询","value":"Search"},
  {"text":"编辑","value":"Update"}
]');
SET @Icon = COALESCE(NULLIF(@Icon, N''), N'el-icon-warning-outline');
SET @OrderNo = COALESCE(@OrderNo, (SELECT ISNULL(MAX(OrderNo), 0) + 1 FROM dbo.Sys_Menu WHERE ParentId = @ParentId));
SET @AuthData = COALESCE(@AuthData, 0);
SET @LinkType = COALESCE(@LinkType, 0);

SELECT TOP (1)
    @NewMenuId = Menu_Id
FROM dbo.Sys_Menu
WHERE Url = N'/WZ_CapacityScheduleAdjustment'
   OR TableName = N'WZ_CapacityScheduleAdjustment'
   OR MenuName = N'异常排产调整工作台'
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
        N'异常排产调整工作台',
        N'WZ_CapacityScheduleAdjustment',
        N'/WZ_CapacityScheduleAdjustment',
        @Auth,
        @AuthData,
        @LinkType,
        N'独立处理排产优化日期超载和法定节假日异常，并同步排产优化汇总。',
        @Icon,
        @OrderNo,
        N'Codex',
        GETDATE(),
        1,
        0
    );

    SET @NewMenuId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    UPDATE dbo.Sys_Menu
    SET ParentId = @ParentId,
        MenuName = N'异常排产调整工作台',
        TableName = N'WZ_CapacityScheduleAdjustment',
        Url = N'/WZ_CapacityScheduleAdjustment',
        Auth = CASE WHEN NULLIF(Auth, N'') IS NULL THEN @Auth ELSE Auth END,
        AuthData = COALESCE(AuthData, @AuthData),
        LinkType = COALESCE(LinkType, @LinkType),
        Icon = COALESCE(NULLIF(Icon, N''), @Icon),
        OrderNo = COALESCE(OrderNo, @OrderNo),
        Enable = 1,
        MenuType = 0,
        Modifier = N'Codex',
        ModifyDate = GETDATE()
    WHERE Menu_Id = @NewMenuId;
END;

IF OBJECT_ID(N'dbo.Sys_RoleAuth', N'U') IS NOT NULL
BEGIN
    IF @SourceMenuId IS NOT NULL
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
    END;

    IF OBJECT_ID(N'dbo.Sys_Role', N'U') IS NOT NULL
    BEGIN
        DECLARE @TargetRoles TABLE(Role_Id INT NOT NULL PRIMARY KEY);
        DECLARE @TargetNames TABLE(Name NVARCHAR(50) NOT NULL PRIMARY KEY);

        INSERT INTO @TargetNames(Name)
        VALUES (N'辛防'), (N'任新'), (N'川仪管理员');

        INSERT INTO @TargetRoles(Role_Id)
        SELECT DISTINCT r.Role_Id
        FROM dbo.Sys_Role AS r
        WHERE EXISTS
        (
            SELECT 1
            FROM @TargetNames AS targetName
            WHERE targetName.Name = LTRIM(RTRIM(r.RoleName))
        )
          AND ISNULL(r.Enable, 1) = 1;

        IF OBJECT_ID(N'dbo.Sys_User', N'U') IS NOT NULL
        BEGIN
            INSERT INTO @TargetRoles(Role_Id)
            SELECT DISTINCT roleId
            FROM
            (
                SELECT r.Role_Id AS roleId
                FROM dbo.Sys_User AS u
                INNER JOIN dbo.Sys_Role AS r
                    ON CHARINDEX(
                        N',' + CONVERT(NVARCHAR(20), r.Role_Id) + N',',
                        N',' + REPLACE(ISNULL(u.RoleIds, N''), N' ', N'') + N','
                    ) > 0
                WHERE EXISTS
                (
                    SELECT 1
                    FROM @TargetNames AS targetName
                    WHERE targetName.Name = LTRIM(RTRIM(u.UserTrueName))
                       OR targetName.Name = LTRIM(RTRIM(u.UserName))
                )
                  AND ISNULL(r.Enable, 1) = 1

                UNION ALL

                SELECT u.Role_Id AS roleId
                FROM dbo.Sys_User AS u
                WHERE EXISTS
                  (
                      SELECT 1
                      FROM @TargetNames AS targetName
                      WHERE targetName.Name = LTRIM(RTRIM(u.UserTrueName))
                         OR targetName.Name = LTRIM(RTRIM(u.UserName))
                  )
                  AND u.Role_Id IS NOT NULL
            ) AS source
            WHERE roleId IS NOT NULL
              AND NOT EXISTS (SELECT 1 FROM @TargetRoles AS target WHERE target.Role_Id = source.roleId);
        END;

        IF OBJECT_ID(N'dbo.Sys_UserRole', N'U') IS NOT NULL
           AND OBJECT_ID(N'dbo.Sys_User', N'U') IS NOT NULL
        BEGIN
            INSERT INTO @TargetRoles(Role_Id)
            SELECT DISTINCT ur.RoleId
            FROM dbo.Sys_User AS u
            INNER JOIN dbo.Sys_UserRole AS ur ON ur.UserId = u.User_Id
            WHERE EXISTS
              (
                  SELECT 1
                  FROM @TargetNames AS targetName
                  WHERE targetName.Name = LTRIM(RTRIM(u.UserTrueName))
                     OR targetName.Name = LTRIM(RTRIM(u.UserName))
              )
              AND ISNULL(ur.Enable, 1) = 1
              AND NOT EXISTS (SELECT 1 FROM @TargetRoles AS target WHERE target.Role_Id = ur.RoleId);
        END;

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
            target.Role_Id,
            NULL,
            @NewMenuId,
            @TargetAuthValue,
            NULL,
            N'Codex',
            GETDATE()
        FROM @TargetRoles AS target
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM dbo.Sys_RoleAuth AS auth
            WHERE auth.Menu_Id = @NewMenuId
              AND auth.Role_Id = target.Role_Id
        );

        UPDATE auth
        SET AuthValue = @TargetAuthValue,
            Modifier = N'Codex',
            ModifyDate = GETDATE()
        FROM dbo.Sys_RoleAuth AS auth
        INNER JOIN @TargetRoles AS target ON target.Role_Id = auth.Role_Id
        WHERE auth.Menu_Id = @NewMenuId
          AND ISNULL(auth.AuthValue, N'') <> @TargetAuthValue;
    END;
END;

PRINT N'已将异常排产调整工作台校准到“计划管理”，并授权辛防、任新、川仪管理员使用。';
GO
