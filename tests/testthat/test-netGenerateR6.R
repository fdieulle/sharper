library(sharper)
library(testthat)

context("Generate R6 classes")

package_folder <- path.package("sharper")
netLoadAssembly(file.path(package_folder, "tests", "AssemblyForTests.dll"))

tmp_folder <- tempdir()

test_that("Generate simple class", {
  
  file_path <- file.path(tmp_folder, "AutoGenerate-Simple-R6.R")
  netGenerateR6("AssemblyForTests.DefaultCtorData", file_path)
  
  source(file_path)
  
  o <- DefaultCtorData$new(Name = "My name")
  expect_equal(o$Name, "My name")
  expect_equal(class(o), c("DefaultCtorData", "NetObject", "R6"))
  
  o$Integers <- c(11L, 12L)
  expect_equal(o$Integers, c(11L, 12L))
  
  x <- netNew("AssemblyForTests.DefaultCtorData")
  o <- DefaultCtorData$new(ptr = x)
  o$Name = "foo"
  expect_equal(o$Name, "foo")
  
  clone <- o$Clone()
  expect_equal(clone$Name, "foo")
  expect_equal(class(clone), c("DefaultCtorData", "NetObject", "R6"))
  
  value = NULL
  result <- o$TryGetValue(value)
  expect_true(result)
  expect_equal(value, 12.4)
  
  object = NULL
  result <- o$TryGetObject(object)
  expect_true(result)
  expect_equal(class(object), c("DefaultCtorData", "NetObject", "R6"))
  expect_equal(object$Name, "Out object")
  
  o$UpdateValue(value)
  expect_equal(value, 13.4)
  
  object = NULL
  o$UpdateObject(object)
  expect_equal(class(object), c("DefaultCtorData", "NetObject", "R6"))
  expect_equal(object$Name, "Ref object")
})

test_that("Generate class with ctor parameter", {
  
  file_path <- file.path(tmp_folder, "AutoGenerate-CtorArg-R6.R")
  netGenerateR6("AssemblyForTests.OneCtorData", file_path)
  
  source(file_path)
  
  o <- OneCtorData$new(10L)
  expect_equal(o$Id, 10L)
  expect_equal(class(o), c("OneCtorData", "NetObject", "R6"))
  
  o$Id <- 11L
  expect_equal(o$Id, 11L)
  
  x <- netNew("AssemblyForTests.OneCtorData", 12L)
  o <- OneCtorData$new(ptr = x)
  expect_equal(o$Id, 12L)
  
  o <- OneCtorData$new(13L, Name = "My name")
  expect_equal(o$Id, 13L)
  expect_equal(o$Name, "My name")
})

test_that("Generate inherited types from C# interface", {
  file_path <- file.path(tmp_folder, "AutoGenerate-IData-R6.R")
  netGenerateR6("AssemblyForTests.IData", file_path, withInheritedTypes = TRUE)
  
  source(file_path)
  
  o <- DefaultCtorData$new(Name = "Test")
  expect_equal(o$Name, "Test")
  o$Name = "Updated property"
  expect_equal(o$Name, "Updated property")
  
  o <- ManyCtorData$new(Name = "MyName", Id = 123L)
  expect_equal(o$Name, "MyName")
  expect_equal(o$Id, 123L)
})
