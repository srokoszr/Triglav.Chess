#pragma once
#include "TypeAliases.h"

typedef struct Coordinates
{
    int32_t SingleCoordinate;

    int32_t Row;

    int32_t Column;
} Coordinates;

#define COORDINATES_LOWER_PART_MASK 0x07

#define GetSingleCoordinate(row, column) (((row) << 3) | (column))
#define CoordinatesGetRow(data) (data >> 3)
#define CoordinatesGetColumn(data) (data & COORDINATES_LOWER_PART_MASK)

#define GetCoordinates(data) { data >> 3 , data & COORDINATES_LOWER_PART_MASK }