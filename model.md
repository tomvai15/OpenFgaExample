model
  schema 1.1

type user

type Organization
  relations
    define Admin: [user]
    define CanCreate: Admin
    define CanView: Member or Viewer or Admin
    define Member: [user]
    define Viewer: [user]

type Project
  relations
    define CanDelete: CanEdit
    define CanEdit: Owner or Editor
    define CanView: Viewer or CanEdit or CanView from Organization
    define Editor: [user]
    define Organization: [Organization]
    define Owner: [user]
    define Viewer: [user]
