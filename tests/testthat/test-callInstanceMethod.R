library(sharper)
library(testthat)

context("call instance methods")

package_folder = path.package("sharper")
assembly_file <- file.path(package_folder, "tests", "AssemblyForTests.dll")
assembly_file <- "C:/OtherDrive/Workspace/Git/fdieulle/sharper/tests/dotnet/AssemblyForTests/bin/Debug/netstandard2.0/AssemblyForTests.dll"
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
  x_ref <-netGet(x, "OneCtorData")
  expect_equal(x_ref, ref)
  expect_equal(netCall(x_ref, "ToString"), netCall(ref, "ToString"))
})