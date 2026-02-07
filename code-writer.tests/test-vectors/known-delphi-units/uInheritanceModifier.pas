unit uInheritanceModifier;

{$IFDEF FPC}
  {$MODE DELPHI}
{$ENDIF}

interface

type
  ClassX = class abstract
  end;

type
  ClassY = class sealed(ClassX)
  end;

implementation

end.
