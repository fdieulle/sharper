#' @description 
#' {{Description}}
#' {{Parameters_doc}}
#' @return {{Return_doc}}
{{MethodName}} = function ({{Parameters}}{{Comma}}wrap = TRUE, out_env = parent.frame()) {
  if ({{HasByRefParams}}) {
    Call <- match.call()
	result <- self$call("{{MethodName}}", {{Parameters}}, wrap = wrap)
	call_names <- names(Call)
	for (i in 2:length(Call)) {
		if (call_names[i] == "wrap" || call_names[i] == "out_env") next
		assign(deparse(Call[[i]]), get(call_names[i]), env = out_env)
	}
	{{Return}} (result)
  } else {
    {{Return}} (self$call("{{MethodName}}"{{Comma}}{{Parameters}}, wrap = wrap, out_env = out_env))
  }
}