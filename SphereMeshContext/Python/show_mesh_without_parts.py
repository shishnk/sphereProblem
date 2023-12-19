import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d.art3d import Poly3DCollection

with open('points', 'r') as file:
    lines = file.readlines()
    points = np.array([list(map(float, line.split())) for line in lines])

with open('parallelepipeds', 'r') as file:
    lines = file.readlines()
    parallelepiped_points = [list(map(int, line.split())) for line in lines]

with open('prisms', 'r') as file:
    lines = file.readlines()
    prism_points = [list(map(int, line.split())) for line in lines]

fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')

for p_points in parallelepiped_points:
    Z = np.array([
        [points[p_points[0]], points[p_points[1]], points[p_points[5]], points[p_points[4]]],
        [points[p_points[7]], points[p_points[6]], points[p_points[2]], points[p_points[3]]],
        [points[p_points[0]], points[p_points[1]], points[p_points[3]], points[p_points[2]]],
        [points[p_points[7]], points[p_points[6]], points[p_points[4]], points[p_points[5]]],
        [points[p_points[7]], points[p_points[3]], points[p_points[1]], points[p_points[5]]],
        [points[p_points[0]], points[p_points[2]], points[p_points[6]], points[p_points[4]]]
    ])
    ax.add_collection3d(Poly3DCollection(Z, facecolors='cyan', linewidths=1, edgecolors='r', alpha=.25))

for p_points in prism_points:
    Z = np.array([
        [points[p_points[0]], points[p_points[1]], points[p_points[4]], points[p_points[3]]],
        [points[p_points[2]], points[p_points[5]], points[p_points[4]], points[p_points[1]]],
        [points[p_points[2]], points[p_points[0]], points[p_points[3]], points[p_points[5]]]
    ])
    ax.add_collection3d(Poly3DCollection(Z, facecolors='cyan', linewidths=1, edgecolors='r', alpha=.25))

ax.scatter(points[:, 0],points[:, 1], points[:, 2])

for i in range(len(points)):
    ax.text(points[i, 0], points[i, 1], points[i, 2], str(i))

plt.show()