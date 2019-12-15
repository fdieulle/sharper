#' {{TypeName}} R6 class
#'
#' @description
#' {{Description}}
#' 
#' @md
#' @export
{{TypeName}} <- R6Class("{{TypeName}}",
  inherit = {{InheritTypeName}},
  active = list(
    {{Properties}}
  ),
  public = list(
    {{Ctor}}{{Comma}}
    {{Methods}}
  )
)