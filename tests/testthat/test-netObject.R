library(sharper)
library(testthat)

print("Wrap .Net externalptr into R6 NetObject class")
context("Wrap .Net externalptr into R6 NetObject class")

package_folder = path.package("sharper")
assembly_file <- file.path(package_folder, "tests", "AssemblyForTests.dll")
netLoadAssembly(assembly_file)

test_that("Instanciate a NetObject", {
  
  x <- netNew("AssemblyForTests.DefaultCtorData")
  o <- NetObject$new(ptr = x)
  
  expect_null(o$get("Name"))
  o$set("Name", "My Name")
  expect_equal(o$get("Name"), "My Name")
  
  o <- NetObject$new("AssemblyForTests.DefaultCtorData")
  
  expect_null(o$get("Name"))
  o$set("Name", "My Name")
  expect_equal(o$get("Name"), "My Name")
  
  o <- NetObject$new("AssemblyForTests.DefaultCtorData", Name = "My Name")
  expect_equal(o$get("Name"), "My Name")
  
  o <- NetObject$new(ptr = x, Name = "My Name")
  expect_equal(o$get("Name"), "My Name")
})

test_that("NetObject call method", {
  o <- NetObject$new("AssemblyForTests.DefaultCtorData", Name = "My Name")
  
  to_string <- o$call("ToString")
  expect_equal(to_string, "AssemblyForTests.DefaultCtorData")
  
  clone <- o$call("Clone")
  expect_equal("My Name", clone$get("Name"))
  expect_equal(o$get("Name"), clone$get("Name"))
})

test_that("Play with NetObject properties", {
  
  o <- NetObject$new("AssemblyForTests.DefaultCtorData")
  name <- o$get("Name")
  expect_null(name)
  
  o$set("Name", "Test")
  name <- o$get("Name")
  expect_equal(name, "Test")
  
  integers <- o$get("Integers")
  expect_null(integers)
  
  integers <- c(12L, 23L)
  o$set("Integers", integers)
  o_integers <- o$get("Integers")
  expect_equal(o_integers, integers)
  
  ref <- o$get("OneCtorData")
  expect_null(ref)
  
  ref <- netNew("AssemblyForTests.OneCtorData", 21L)
  o$set("OneCtorData", ref)
  o_ref <- o$get("OneCtorData")
  expect_true(inherits(o_ref, "NetObject"))
  expect_equal(o_ref$Ptr, ref)
  
  expect_equal(o_ref$call("ToString"), netCall(ref, "ToString"))
})

test_that("Call method with out argument", {
  o <- NetObject$new("AssemblyForTests.DefaultCtorData", Name = "My Name")
  
  value <- 1.0
  result <- o$call("TryGetValue", value)
  expect_true(result)
  expect_equal(value, 12.4)
  
  object <- NULL
  result <- o$call("TryGetObject", object)
  expect_true(result)
  expect_equal(object$get("Name"), "Out object")
  
  clone <- NULL
  m <- matrix(nrow = 7, ncol = 3, data = 5.1)
  result <- o$call("Clone", m, clone)
  expect_equal(result, m)
  expect_true(inherits(clone, "NetObject"))
  expect_equal(clone$get("Name"), "My Name")
})

test_that("Call method with wrap", {
  o <- NetObject$new("AssemblyForTests.DefaultCtorData", Name = "Test")
  
  result <- o$call("Clone", wrap = FALSE)
  expect_true(inherits(result, "externalptr"))
  expect_equal(netGet(result, "Name"), "Test")
  
  result <- o$call("Clone")
  expect_true(inherits(result, "NetObject"))
  expect_equal(netGet(result, "Name"), "Test")
  
  parameter <- matrix(nrow = 7, ncol = 3, data = 5.1)
  out_x = NULL
  result <- o$call("Clone", parameter, out_x, wrap = FALSE)
  expect_equal(result, parameter)
  expect_true(inherits(out_x, "externalptr"))
  expect_equal(netGet(out_x, "Name"), "Test")
  
  out_object = NULL
  result <- o$call("Clone", parameter, out_object)
  expect_equal(result, parameter)
  expect_true(inherits(out_object, "NetObject"))
  expect_equal(out_object$get("Name"), "Test")
})