library(sharper)
library(testthat)

context("call instance methods")

package_folder = path.package("sharper")
assembly_file <- file.path(package_folder, "tests", "AssemblyForTests.dll")
netLoadAssembly(assembly_file)

test_that("Instanciate a .Net object", {
  
  x <- netNew("AssemblyForTests.DefaultCtorData")
  expect_type(x, "externalptr")
  s <- netCall(x, "ToString")
  expect_equal(s, "AssemblyForTests.DefaultCtorData")
  
  x <- netNew("AssemblyForTests.OneCtorData", 21L)
  expect_type(x, "externalptr")
  s <- netCall(x, "ToString")
  expect_equal(s, "AssemblyForTests.OneCtorData #21")
  
  x <- netNew("AssemblyForTests.ManyCtorData")
  expect_type(x, "externalptr")
  s <- netCall(x, "ToString")
  expect_equal(s, "AssemblyForTests.ManyCtorData Name=Default ctor #-1, Id=-1")
  
  x <- netNew("AssemblyForTests.ManyCtorData", 56L)
  expect_type(x, "externalptr")
  s <- netCall(x, "ToString")
  expect_equal(s, "AssemblyForTests.ManyCtorData Name=Integer ctor #56, Id=56")
  
  x <- netNew("AssemblyForTests.ManyCtorData", "foo")
  expect_type(x, "externalptr")
  s <- netCall(x, "ToString")
  expect_equal(s, "AssemblyForTests.ManyCtorData Name=String Ctor foo, Id=0")
})

test_that("Play with .Net object properties", {
  
  x <- netNew("AssemblyForTests.DefaultCtorData")
  x_name <- netGet(x, "Name")
  expect_null(x_name)
  
  netSet(x, "Name", "Test")
  x_name <- netGet(x, "Name")
  expect_equal(x_name, "Test")
  
  x_integers <- netGet(x, "Integers")
  expect_null(x_integers)
  
  integers <- c(12L, 23L)
  netSet(x, "Integers", integers)
  x_integers <- netGet(x, "Integers")
  expect_equal(x_integers, integers)
  
  x_ref <- netGet(x, "OneCtorData")
  expect_null(x_ref)
  
  ref <- netNew("AssemblyForTests.OneCtorData", 21L)
  netSet(x, "OneCtorData", ref)
  x_ref <- netGet(x, "OneCtorData")
  expect_equal(x_ref, ref)
  expect_equal(netCall(x_ref, "ToString"), netCall(ref, "ToString"))
})

test_that("Dispose .Net object wrapper after R GC", {
  x <- netNew("AssemblyForTests.DefaultCtorData")
  x <- NULL
  gc()
})

test_that("Call method with out argument", {
  x <- netNew("AssemblyForTests.DefaultCtorData")
  
  value <- 1.0
  result <- netCall(x, "TryGetValue", value)
  expect_true(result)
  expect_equal(value, 12.4)
  
  object <- NULL
  result <- netCall(x, "TryGetObject", object)
  expect_true(result)
  expect_equal(netGet(object, "Name"), "Out object")
})

test_that("Call method with ref argument", {
  x <- netNew("AssemblyForTests.DefaultCtorData")
  
  value <- 1.0
  netCall(x, "UpdateValue", value)
  expect_equal(value, 2.0)
  
  object <- NULL
  netCall(x, "UpdateObject", object)
  expect_equal(netGet(object, "Name"), "Ref object")
})

test_that("Call method with wrap", {
  x <- netNew("AssemblyForTests.DefaultCtorData")
  netSet(x, "Name", "Test")
  object <- NetObject$new(ptr = x)
  
  result <- netCall(x, "Clone")
  expect_true(inherits(result, "externalptr"))
  expect_equal(netGet(result, "Name"), "Test")
  
  result <- netCall(object, "Clone")
  expect_true(inherits(result, "externalptr"))
  expect_equal(netGet(result, "Name"), "Test")
  
  result <- netCall(x, "Clone", wrap = TRUE)
  expect_true(inherits(result, "NetObject"))
  expect_equal(netGet(result, "Name"), "Test")
  
  result <- netCall(object, "Clone", wrap = TRUE)
  expect_true(inherits(result, "NetObject"))
  expect_equal(netGet(result, "Name"), "Test")
  
  parameter <- matrix(nrow = 7, ncol = 3, data = 5.1)
  out_x = x
  result <- netCall(x, "Clone", parameter, out_x)
  expect_equal(result, parameter)
  expect_true(inherits(out_x, "externalptr"))
  expect_equal(netGet(out_x, "Name"), "Test")
  
  out_object = object
  result <- netCall(object, "Clone", parameter, out_object)
  expect_equal(result, parameter)
  expect_true(inherits(out_object, "externalptr"))
  expect_equal(netGet(out_object, "Name"), "Test")
  
  result <- netCall(x, "Clone", parameter, out_x, wrap = TRUE)
  expect_equal(result, parameter)
  expect_true(inherits(out_x, "NetObject"))
  expect_equal(out_x$get("Name"), "Test")
  
  result <- netCall(object, "Clone", parameter, out_object, wrap = TRUE)
  expect_equal(result, parameter)
  expect_true(inherits(out_object, "NetObject"))
  expect_equal(out_object$get("Name"), "Test")
})
