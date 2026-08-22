# Page Override: Ticket View

> Overrides `MASTER.md` for `/Tickets/View.aspx`.

## Layout

**50/50 split** with bento-styled cards:

| Side | Content |
|------|---------|
| **Left (50%)** | Current Status, Allocated Hours, Change Status, Client Attachments |
| **Right (50%)** | Description, Upload Attachment |
| **Full width below** | Support Attachments |

Keep CGI brand card headers and soft bento card chrome (radius, shadow, hover).

## Responsive

- ≥992px: side-by-side 50/50  
- <992px: stack (left column first, then right)

## Behavior (unchanged)

- Description editable + Save + last-updated meta  
- Status / Allocated Hours: Support/Admin only  
- Attachments: Client vs Support/Admin split; delete = uploader or Admin  
