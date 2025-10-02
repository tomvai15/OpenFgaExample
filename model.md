model
  schema 1.1

type user

type Organization
  relations
    define Member: [user]
    define CanView: Member
    define CanCreate: Member

type Project
  relations
    define CanEdit: Owner
    define CanDelete: CanEdit
    define CanView: Viewer or CanEdit or Member from Organization
    define Organization: [Organization]
    define Owner: [user]
    define Viewer: [user]
