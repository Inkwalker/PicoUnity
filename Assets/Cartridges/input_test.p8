pico-8 cartridge // http://www.pico-8.com
version 16
__lua__
function _init()
end

function _update()

end

lut = {'\139','\145','\148','\131','\142','\151'}

function _draw()
  cls()

  print('input test', 1, 1, 7)

  print('p0:', 1, 12, 7)
  print('p1:', 1, 18, 7)

  for i = 0, 7, 1 do
    c = 5
    if (btn(i, 0)) c = 8
    if (btnp(i, 0)) c = 10

    print(lut[i + 1], 16 + 8 * i, 12, c)
  end

  for i = 0, 7, 1 do
    c = 5
    if (btn(i, 1)) c = 3
    if (btnp(i, 1)) c = 11

    print(lut[i + 1], 16 + 8 * i, 18, c)
  end
end