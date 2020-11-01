unit uNestedConstants;

{$IFDEF FPC}
  {$MODE DELPHI}
{$ENDIF}

interface

type
  ClassX = class
    const ConstX = 1;

    /// <summary>
    /// This is a true constant used for testing.
    /// </summary>
    private const ConstY = '2';
  end;

implementation

end.
