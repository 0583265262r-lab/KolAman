import geopandas as gpd
from shapely.geometry import Point


def get_region_with_geopandas(file_path: str, lon: float, lat: float) -> str:
    gdf = gpd.read_file(file_path)
    pt = Point(lon, lat)
    matched = gdf[gdf.geometry.contains(pt)]
    if not matched.empty:
        return matched.iloc[0]["region"]
    return "OVERSEAS"

