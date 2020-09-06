unit UnitX;

interface

type
  ClassX = class
  var FieldX: Integer;

  var FieldY: Integer;

  property PropertyX: Integer read FieldX;

  property PropertyY: Integer write FieldY;

  property PropertyXY: Integer read FieldY write FieldX;
  end;

implementation

end.
