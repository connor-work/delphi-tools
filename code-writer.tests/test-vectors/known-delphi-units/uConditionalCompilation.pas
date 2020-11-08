unit uConditionalCompilation;

{$INCLUDE testIncludeFile.inc}

{$IFDEF FPC}
  {$MODE DELPHI}
{$ENDIF}

interface

uses
{$IFDEF TEST_FILE_INCLUDED}
  uReferenced,
{$ENDIF}
{$IFNDEF TEST_FILE_INCLUDED}
  uReferenced2,
{$ENDIF}
{$IFDEF TEST_FILE_INCLUDED}
  uReferenced2;
{$ELSE}
  uReferenced;
{$ENDIF}

type
{$IFDEF TEST_FILE_INCLUDED}
  [volatile]
{$ENDIF}
{$IFNDEF TEST_FILE_INCLUDED}
  [Example]
{$ENDIF}
{$IFDEF TEST_FILE_INCLUDED}
  [Example]
{$ELSE}
  [volatile]
{$ENDIF}
  ClassX = class
  end;

implementation

end.
