INSERT INTO AspNetRoles (ID, Name, NormalizedName)
VALUES (1, 'Admin', 'ADMIN');

INSERT INTO AspNetUserRoles (UserId, RoleId)
Select  u.Id, r.Id from AspNetUsers u
                            JOIN AspNetRoles r
WHERE u.NormalizedEmail in ('EDUARDO.DIAS092@GMAIL.COM', 'AICBRASILL@GMAIL.COM')
  AND r.NormalizedName = 'ADMIN';