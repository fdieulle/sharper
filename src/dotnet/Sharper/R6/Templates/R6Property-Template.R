#' @field {{PropertyName}} {{Description}}
{{PropertyName}} = function(value) {
  if (missing(value)) { return(self$get("{{PropertyName}}")) }
  else { invisible(self$set("{{PropertyName}}", value)) }
}