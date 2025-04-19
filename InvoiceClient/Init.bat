@echo off
CD %~dp0
if not exist %1\PreInvoiceFailure (
    mklink /J %1\PreInvoiceFailure "%1\PreInvoice(Failure)"
) 
if not exist %1\PreInvoiceResponse (
    mklink /J %1\PreInvoiceResponse "%1\PreInvoice(Response)"
)
if not exist %1\CancelInvoiceFailure (
    mklink /J %1\CancelInvoiceFailure "%1\CancelInvoice(Failure)"
) 
if not exist %1\CancelInvoiceResponse (
    mklink /J %1\CancelInvoiceResponse "%1\CancelInvoice(Response)"
) 
if not exist %1\AllowanceFailure (
    mklink /J %1\AllowanceFailure "%1\Allowance(Failure)"
) 
if not exist %1\AllowanceResponse (
    mklink /J %1\AllowanceResponse "%1\Allowance(Response)"
) 
if not exist %1\Replacement (
    mklink /J %1\Replacement .\logs\InvoiceNoInspector\Replacement
) 
if not exist %1\BlindReturn (
    mklink /J %1\BlindReturn .\logs\InvoiceNoInspector\BlindReturn
) 
if not exist %1\ReprintReceipt (
    mklink /J %1\ReprintReceipt .\logs\InvoiceNoInspector\ReprintReceipt
) 
if not exist %1\ZeroAmountReceipt (
    mklink /J %1\ZeroAmountReceipt .\logs\InvoiceNoInspector\ZeroAmountReceipt
) 
