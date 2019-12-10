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

testCallStatic <- function(typeName, methodName, parameter, expected = NULL) {
	x <- netCallStatic(typeName, methodName, parameter)
	expect_equal(class(x), class(parameter))
	if (is.null(expected))
		expect_equal(x, parameter)
	else expect_equal(x, expected)
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

	# Difftime
	t1 <- as.POSIXct("2019-03-20 10:32:21")
	t2 <- as.POSIXct("2019-04-20 10:42:00")
	expected <- difftime(t2, t1, units = "secs")
	
	testCallStatic(typeName, methodName, difftime(t2, t1, units = "auto"), expected)
	testCallStatic(typeName, methodName, difftime(t2, t1, units = "secs"), expected)
	testCallStatic(typeName, methodName, difftime(t2, t1, units = "mins"), expected)
	testCallStatic(typeName, methodName, difftime(t2, t1, units = "hours"), expected)
	testCallStatic(typeName, methodName, difftime(t2, t1, units = "days"), expected)
	testCallStatic(typeName, methodName, difftime(t2, t1, units = "weeks"), expected)

	x <- seq(Sys.time(), by = '10 min', length = 10)
	expected <- difftime(head(x, -1), tail(x, -1), units = "secs")
	
	testCallStatic(typeName, methodName, difftime(head(x, -1), tail(x, -1), units = "auto"), expected)
	testCallStatic(typeName, methodName, difftime(head(x, -1), tail(x, -1), units = "secs"), expected)
	testCallStatic(typeName, methodName, difftime(head(x, -1), tail(x, -1), units = "mins"), expected)
	testCallStatic(typeName, methodName, difftime(head(x, -1), tail(x, -1), units = "hours"), expected)
	testCallStatic(typeName, methodName, difftime(head(x, -1), tail(x, -1), units = "days"), expected)
	testCallStatic(typeName, methodName, difftime(head(x, -1), tail(x, -1), units = "weeks"), expected)

	x <- seq(Sys.time(), by = '10 min', length = 16)
	expected <- as.difftime(matrix(nrow = 3, ncol = 5, data = difftime(head(x, -1), tail(x, -1))), units="mins")
	units(expected) <- "secs"
	input <- expected
	
	units(input) <- "secs"
	testCallStatic(typeName, methodName, input, expected)
	units(input) <- "mins"
	testCallStatic(typeName, methodName, input, expected)
	units(input) <- "hours"
	testCallStatic(typeName, methodName, input, expected)
	units(input) <- "days"
	testCallStatic(typeName, methodName, input, expected)
	units(input) <- "weeks"
	testCallStatic(typeName, methodName, input, expected)
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

test_that("Call static method with out argument", {
  
  typeName = "AssemblyForTests.StaticClass"
  
  value <- 1.0
  result <- netCallStatic(typeName, "TryGetValue", value)
  expect_true(result)
  expect_equal(value, 12.4)
  
  object <- NULL
  result <- netCallStatic(typeName, "TryGetObject", object)
  expect_true(result)
  expect_equal(netGet(object, "Name"), "Out object")
})

test_that("Call static method with ref argument", {
  typeName = "AssemblyForTests.StaticClass"
  
  value <- 1.0
  netCallStatic(typeName, "UpdateValue", value)
  expect_equal(value, 2.0)
  
  object <- NULL
  netCallStatic(typeName, "UpdateObject", object)
  expect_equal(netGet(object, "Name"), "Ref object")
})