library(sharper)
library(testthat)

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
  
  o <- NetObject$new(typeName = "AssemblyForTests.DefaultCtorData")
  
  expect_null(o$get("Name"))
  o$set("Name", "My Name")
  expect_equal(o$get("Name"), "My Name")
  
  o <- NetObject$new(typeName = "AssemblyForTests.DefaultCtorData", Name = "My Name")
  expect_equal(o$get("Name"), "My Name")
  
  o <- NetObject$new(ptr = x, Name = "My Name")
  expect_equal(o$get("Name"), "My Name")
})

test_that("NetObject call method", {
  o <- NetObject$new(typeName = "AssemblyForTests.DefaultCtorData", Name = "My Name")
  
  to_string <- o$call("ToString")
  expect_equal(to_string, "AssemblyForTests.DefaultCtorData")
  
  value = 1
  result <- o$call("TryGetValue", value)
  expect_true(result)
  expect_equal(value, 12.4)
  
  object = NULL
  result <- o$call("TryGetObject", object)
  expect_true(result)
  expect_equal(object$get("Name"), "Out object")
})
