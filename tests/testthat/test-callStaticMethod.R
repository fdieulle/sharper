context("call static methods")

test_that("Call the same static method with different signatures", {
	typeName = "AssemblyForTests.StaticClass"
	methodName = "SameMethodName"

	netCallStatic(typeName, methodName)
  
	netCallStatic(typeName, methodName, 1L)
	netCallStatic(typeName, methodName, 1.23)
	netCallStatic(typeName, methodName, c(2L, 3L))
	netCallStatic(typeName, methodName, c(1.24, 1.25))
  
	netCallStatic(typeName, methodName, 2.13, 1L)
	netCallStatic(typeName, methodName, 2.14, c(2L, 3L))
	netCallStatic(typeName, methodName, c(1.24, 1.25), 14L)
	netCallStatic(typeName, methodName, c(1.24, 1.25), c(14L, 15L))
})

testCallStatic <- function(typeName, methodName, parameter) {
	x <- netCallStatic(typeName, methodName, parameter)
	expect_equal(class(x), class(parameter))
	expect_equal(x, parameter)
}

test_that("Call static method for native types", {
	typeName = "AssemblyForTests.StaticClass"
	methodName = "ReturnsNativeType"

	# Integer
	testCallStatic(typeName, methodName, 2L)
	testCallStatic(typeName, methodName, c(2L, 3L, 4L))
	testCallStatic(typeName, methodName, matrix(nrow = 7, ncol = 3, data = 5L))

	# Numeric
	testCallStatic(typeName, methodName, 2.1)
	testCallStatic(typeName, methodName, c(2.1, 3.1, 4.1))
	testCallStatic(typeName, methodName, matrix(nrow = 7, ncol = 3, data = 5.1))

	# Logical
	testCallStatic(typeName, methodName, TRUE)
	testCallStatic(typeName, methodName, c(TRUE, FALSE, TRUE))
	testCallStatic(typeName, methodName, matrix(nrow = 7, ncol = 3, data = TRUE))

	# Character
	testCallStatic(typeName, methodName, "Hello")
	testCallStatic(typeName, methodName, c("Hello", "dotnet", "It' R"))
	# Not supported testCallStatic(typeName, methodName, matrix(nrow = 7, ncol = 3, data = "Hello"))
})

testTimezone <- function(timezone, convertedtimezone = NULL) {
  if(is.null(convertedtimezone)) {
    convertedtimezone <- timezone
  }
  
  offset = 0
  for(i in 1:1000) {
    offset = offset + 77760000
    p <- as.POSIXct(offset, origin = "1960-01-01", tz = timezone)
    x <- netCallStatic("AssemblyForTests.StaticClass", "ReturnsNativeType", p)
    expect_equal(class(x), class(p))
    expect_equal(attr(x, "tzone"), convertedtimezone)
    d <- as.POSIXct(as.numeric(x), origin = "1970-01-01", tz = timezone)
    expect_equal(x, d)
  }
}

test_that("Date & Time convertion", {
	typeName = "System.DateTime"

	x <- netGetStatic(typeName, "Now")
	expect_equal(class(x), class(Sys.time()))
	expect_equal(attr(x, "tzone"), Sys.timezone(location = TRUE))
	d <- as.POSIXct(as.numeric(x), origin = "1970-01-01", tz = Sys.timezone(location = TRUE))
	expect_equal(x, d)
	
	x <- netGetStatic(typeName, "UtcNow")
	expect_equal(class(x), class(Sys.time()))
	expect_equal(attr(x, "tzone"), "Etc/GMT")
	d <- as.POSIXct(as.numeric(x), origin = "1970-01-01", tz = "Etc/GMT")
	expect_equal(x, d)
	
	testTimezone("Europe/Paris")
	#testTimezone("UTC", "Europe/Paris")
	#testTimezone("America/New_York")
	#testTimezone("UTC", "Europe/Paris")
})